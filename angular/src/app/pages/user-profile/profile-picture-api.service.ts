import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';

@Injectable({ providedIn: 'root' })
export class ProfilePictureApiService {
  private readonly restService = inject(RestService);

  uploadMyProfilePicture(file: File) {
    const formData = new FormData();
    formData.append('input', file, file.name);

    return this.restService.request<any, string>(
      {
        method: 'POST',
        url: '/api/app/user-profile/upload-my-profile-picture',
        body: formData,
      },
      { apiName: 'Default' },
    );
  }

  uploadUserProfilePicture(userId: string, file: File) {
    const formData = new FormData();
    formData.append('input', file, file.name);

    return this.restService.request<any, string>(
      {
        method: 'POST',
        url: `/api/admin/profile-pictures/${userId}`,
        body: formData,
      },
      { apiName: 'Default' },
    );
  }

  deleteMyProfilePicture() {
    return this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: '/api/app/user-profile/my-profile-picture',
      },
      { apiName: 'Default' },
    );
  }
}
