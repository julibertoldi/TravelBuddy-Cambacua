import { Injectable, inject } from '@angular/core';
import { ConfigStateService, EnvironmentService } from '@abp/ng.core';
import { UserProfileService as LeptonUserProfileService } from '@volo/ngx-lepton-x.core';
import { catchError, distinctUntilChanged, of, switchMap } from 'rxjs';
import { UserProfileService as UserProfileApiService } from '../proxy/users/user-profile.service';

@Injectable({ providedIn: 'root' })
export class ProfilePictureHeaderService {
  private readonly configState = inject(ConfigStateService);
  private readonly environmentService = inject(EnvironmentService);
  private readonly leptonUserProfile = inject(LeptonUserProfileService);
  private readonly userProfileApi = inject(UserProfileApiService);

  initialize(): void {
    this.configState.getOne$('currentUser').pipe(
      distinctUntilChanged((previous, current) => previous?.id === current?.id),
      switchMap(user => user?.id
        ? this.userProfileApi.getMyProfile().pipe(catchError(() => of(null)))
        : of(null)),
    ).subscribe(profile => {
      const pictureUrl = profile?.fotoPerfilUrl;
      this.leptonUserProfile.patchUser({
        avatar: pictureUrl
          ? { type: 'image', source: this.toAbsolutePictureUrl(pictureUrl) }
          : { type: 'icon', source: 'bi bi-person-circle' },
      });
    });
  }

  private toAbsolutePictureUrl(url: string): string {
    if (/^(https?:|blob:|data:)/i.test(url)) {
      return url;
    }

    const apiUrl = this.environmentService.getApiUrl('Default').replace(/\/+$/, '');
    return `${apiUrl}/${url.replace(/^\/+/, '')}`;
  }
}