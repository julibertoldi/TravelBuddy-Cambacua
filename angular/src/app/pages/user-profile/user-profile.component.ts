import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ConfigStateService, EnvironmentService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { UserProfileService } from '../../proxy/users/user-profile.service';
import { ProfilePictureApiService, ProfilePictureUploadResult } from './profile-picture-api.service';
import { validateProfilePictureFile } from './profile-picture-validation';
import { Observable, of, switchMap } from 'rxjs';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss']
})
export class UserProfileComponent implements OnInit, OnDestroy {

  private fb = inject(FormBuilder);
  private userProfileService = inject(UserProfileService);
  private profilePictureApi = inject(ProfilePictureApiService);
  private toaster = inject(ToasterService);
  private configState = inject(ConfigStateService);
  private environmentService = inject(EnvironmentService);

  profileForm!: FormGroup;

  loading = true;
  saving = false;
  currentPhotoUrl = '';
  previewUrl = '';
  photoError = '';
  selectedPhoto: File | null = null;
  removePhotoRequested = false;

  ngOnInit(): void {
    this.initForm();
    this.loadProfile();
  }

  private initForm(): void {
    this.profileForm = this.fb.group({
      userName: [{ value: '', disabled: true }],
      email: [{ value: '', disabled: true }],
      nombre: ['', [Validators.required, Validators.maxLength(64)]],
      apellido: ['', [Validators.required, Validators.maxLength(64)]],
      preferencias: ['', [Validators.maxLength(2000)]]
    });
  }

  private loadProfile(): void {
    this.loading = true;

    this.userProfileService.getMyProfile().subscribe({
      next: profile => {
        const currentUser = this.configState.getOne('currentUser');
        this.currentPhotoUrl = profile.fotoPerfilUrl || '';
        this.profileForm.patchValue({
          userName: profile.userName || currentUser?.userName || '',
          email: profile.email || currentUser?.email || '',
          nombre: profile.nombre || currentUser?.name || '',
          apellido: profile.apellido || currentUser?.surName || '',
          preferencias: profile.preferencias || ''
        });

        this.profileForm.markAsPristine();
        this.loading = false;
      },
      error: err => {
        const currentUser = this.configState.getOne('currentUser');
        if (currentUser) {
          this.profileForm.patchValue({
            userName: currentUser.userName || '',
            email: currentUser.email || '',
            nombre: currentUser.name || '',
            apellido: currentUser.surName || ''
          });
          this.profileForm.markAsPristine();
        }
        this.toaster.error(
          err.error?.error?.message ?? 'No se pudo cargar el perfil.',
          'Error'
        );
        this.loading = false;
      }
    });
  }

  getAvatarUrl(): string {
    if (this.previewUrl) {
      return this.previewUrl;
    }

    if (this.currentPhotoUrl && !this.removePhotoRequested) {
      return this.resolveProfilePictureUrl(this.currentPhotoUrl);
    }

    return this.defaultAvatarUrl();
  }

  get photoChanged(): boolean {
    return this.selectedPhoto !== null || this.removePhotoRequested;
  }

  handleImageError(event: Event): void {
    (event.target as HTMLImageElement).src = this.defaultAvatarUrl();
  }

  private defaultAvatarUrl(): string {
    const nombre = this.profileForm?.get('nombre')?.value?.trim() || '';
    const apellido = this.profileForm?.get('apellido')?.value?.trim() || '';
    const nombreCompleto = `${nombre} ${apellido}`.trim() || 'Usuario';
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(nombreCompleto)}&background=a8beb9&color=fff&size=128&bold=true`;
  }

  private resolveProfilePictureUrl(url: string): string {
    if (/^(https?:|blob:|data:)/i.test(url)) {
      return url;
    }

    const apiUrl = this.environmentService.getApiUrl('Default').replace(/\/+$/, '');
    return `${apiUrl}/${url.replace(/^\/+/, '')}`;
  }

  clearPhoto(): void {
    this.revokePreviewUrl();
    this.selectedPhoto = null;
    this.photoError = '';
    this.removePhotoRequested = Boolean(this.currentPhotoUrl);
    this.profileForm.markAsDirty();
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    this.photoError = '';

    if (!file) {
      return;
    }

    const validationError = validateProfilePictureFile(file);
    if (validationError) {
      this.photoError = validationError;
      return;
    }

    this.revokePreviewUrl();
    this.selectedPhoto = file;
    this.removePhotoRequested = false;
    this.previewUrl = URL.createObjectURL(file);
    this.profileForm.markAsDirty();
  }

  saveProfile(): void {
    if (this.saving) {
      return;
    }

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.saving = true;

    // Solo los campos que la consigna permite modificar
    const profileUpdate = {
      nombre: this.profileForm.get('nombre')?.value ?? '',
      apellido: this.profileForm.get('apellido')?.value ?? '',
      preferencias: this.profileForm.get('preferencias')?.value ?? ''
    };

    let photoRequest: Observable<ProfilePictureUploadResult | string | void> = of(this.currentPhotoUrl);
    if (this.selectedPhoto) {
      photoRequest = this.profilePictureApi.uploadMyProfilePicture(this.selectedPhoto);
    } else if (this.removePhotoRequested) {
      photoRequest = this.profilePictureApi.deleteMyProfilePicture().pipe(
        switchMap(() => of('')),
      );
    }

    photoRequest.pipe(
      switchMap(photoResult => {
        const photoUrl = typeof photoResult === 'string'
          ? photoResult
          : photoResult && 'url' in photoResult
            ? photoResult.url
            : undefined;
        if (photoUrl !== undefined) {
          this.currentPhotoUrl = photoUrl;
        }
        return this.userProfileService.updateMyProfile(profileUpdate);
      }),
    ).subscribe({
      next: () => {
        this.toaster.success('Perfil actualizado correctamente.', 'Éxito');
        this.saving = false;
        this.selectedPhoto = null;
        this.removePhotoRequested = false;
        this.revokePreviewUrl();
        this.profileForm.markAsPristine();

        setTimeout(() => {
          window.location.reload();
        }, 1200);
      },
      error: err => {
        this.toaster.error(
          err.error?.error?.message ?? 'Error al actualizar el perfil.',
          'Error'
        );
        this.saving = false;
      }
    });
  }

  onSubmit(): void {
    this.saveProfile();
  }

  ngOnDestroy(): void {
    this.revokePreviewUrl();
  }

  private revokePreviewUrl(): void {
    if (this.previewUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.previewUrl);
    }
    this.previewUrl = '';
  }
}