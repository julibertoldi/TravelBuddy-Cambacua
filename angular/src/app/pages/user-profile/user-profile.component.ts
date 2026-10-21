import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ConfigStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { UserProfileService } from '../../proxy/users/user-profile.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss']
})
export class UserProfileComponent implements OnInit {

  private fb = inject(FormBuilder);
  private userProfileService = inject(UserProfileService);
  private toaster = inject(ToasterService);
  private configState = inject(ConfigStateService);

  profileForm!: FormGroup;

  loading = true;
  saving = false;

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
      fotoPerfilUrl: ['', [Validators.maxLength(512)]],
      preferencias: ['', [Validators.maxLength(2000)]]
    });
  }

  private loadProfile(): void {
    this.loading = true;

    this.userProfileService.getMyProfile().subscribe({
      next: profile => {
        const currentUser = this.configState.getOne('currentUser');
        this.profileForm.patchValue({
          userName: profile.userName || currentUser?.userName || '',
          email: profile.email || currentUser?.email || '',
          nombre: profile.nombre || currentUser?.name || '',
          apellido: profile.apellido || currentUser?.surName || '',
          fotoPerfilUrl: profile.fotoPerfilUrl || '',
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
    const foto = this.profileForm?.get('fotoPerfilUrl')?.value?.trim();
    if (foto) {
      return foto;
    }
    return this.defaultAvatarUrl();
  }

  handleImageError(event: any): void {
    event.target.src = this.defaultAvatarUrl();
  }

  private defaultAvatarUrl(): string {
    const nombre = this.profileForm?.get('nombre')?.value?.trim() || '';
    const apellido = this.profileForm?.get('apellido')?.value?.trim() || '';
    const nombreCompleto = `${nombre} ${apellido}`.trim() || 'Usuario';
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(nombreCompleto)}&background=a8beb9&color=fff&size=128&bold=true`;
  }

  clearPhoto(): void {
    const fotoCtrl = this.profileForm.get('fotoPerfilUrl');
    fotoCtrl?.setValue('');
    fotoCtrl?.markAsDirty();
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
      fotoPerfilUrl: this.profileForm.get('fotoPerfilUrl')?.value ?? '',
      preferencias: this.profileForm.get('preferencias')?.value ?? ''
    };

    this.userProfileService.updateMyProfile(profileUpdate).subscribe({
      next: () => {
        this.toaster.success('Perfil actualizado correctamente.', 'Éxito');
        this.saving = false;
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
}