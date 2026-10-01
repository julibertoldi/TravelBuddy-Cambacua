import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import {
  RestService,
  AuthService,
  ConfigStateService
} from '@abp/ng.core';
import {
  ToasterService,
  ConfirmationService,
  Confirmation
} from '@abp/ng.theme.shared';
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
  private restService = inject(RestService);
  private userProfileService = inject(UserProfileService);
  private authService = inject(AuthService);
  private toaster = inject(ToasterService);
  private confirmation = inject(ConfirmationService);
  private router = inject(Router);
  private configState = inject(ConfigStateService);

  profileForm!: FormGroup;

  loading = true;
  saving = false;
  deleting = false;
  currentUserId: string | null = null;

  ngOnInit(): void {
    this.currentUserId = this.getCurrentUserId();
    this.initForm();
    this.loadProfile();
  }

  getCurrentUserId(): string | null {
    const currentUser = this.configState.getOne('currentUser');
    return currentUser ? currentUser.id : null;
  }

  private initForm(): void {
    this.profileForm = this.fb.group({
      userName: [{ value: '', disabled: true }],
      nombre: ['', [Validators.required, Validators.maxLength(64)]],
      apellido: ['', [Validators.required, Validators.maxLength(64)]],
      email: ['', [Validators.required, Validators.email]],
      fotoPerfilUrl: ['', [Validators.maxLength(1000)]],
      preferencias: ['', [Validators.maxLength(2000)]]
    });
  }

  private loadProfile(): void {
    this.loading = true;

    this.restService.request<any, any>({
      method: 'GET',
      url: '/api/user-profile/me'
    })
    .subscribe({
      next: profile => {
        const currentUser = this.configState.getOne('currentUser');
        this.profileForm.patchValue({
          userName: profile.userName || currentUser?.userName || '',
          nombre: profile.nombre || currentUser?.name || '',
          apellido: profile.apellido || currentUser?.surName || '',
          email: profile.email || currentUser?.email || '',
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
            nombre: currentUser.name || '',
            apellido: currentUser.surName || '',
            email: currentUser.email || ''
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
    const nombre = this.profileForm?.get('nombre')?.value?.trim() || '';
    const apellido = this.profileForm?.get('apellido')?.value?.trim() || '';
    const nombreCompleto = `${nombre} ${apellido}`.trim() || 'Usuario';
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(nombreCompleto)}&background=a8beb9&color=fff&size=128&bold=true`;
  }

  handleImageError(event: any): void {
    const nombre = this.profileForm?.get('nombre')?.value?.trim() || '';
    const apellido = this.profileForm?.get('apellido')?.value?.trim() || '';
    const nombreCompleto = `${nombre} ${apellido}`.trim() || 'Usuario';
    event.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(nombreCompleto)}&background=a8beb9&color=fff&size=128&bold=true`;
  }

  clearPhoto(): void {
    const fotoCtrl = this.profileForm.get('fotoPerfilUrl');
    fotoCtrl?.setValue('');
    fotoCtrl?.markAsDirty();
  }

  // Método de guardado protegido contra doble ejecución concurrente
  saveProfile(): void {
    if (this.saving) {
      return;
    }

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    if (!this.currentUserId) {
      this.toaster.error('Usuario no autenticado.');
      return;
    }

    this.saving = true;

    this.userProfileService.updateMyProfile(
      this.currentUserId,
      this.profileForm.value
    ).subscribe({
      next: () => {
        this.toaster.success(
          'Perfil actualizado correctamente.',
          'Éxito'
        );

        this.saving = false;
        
        // Marca el formulario como sin modificar para opacar el botón de guardar
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

  // Manejador del submit del formulario
  onSubmit(): void {
    this.saveProfile();
  }

  confirmDelete(): void {
    this.confirmation.warn(
      '¿Estás seguro de que deseas eliminar tu cuenta permanentemente? No podrás volver a ingresar con este usuario.',
      'Eliminar cuenta'
    ).subscribe((status: Confirmation.Status) => { 
      if (status === Confirmation.Status.confirm) { 
        this.deleteAccount();
      }
    });
  }

  private deleteAccount(): void {
    if (!this.currentUserId) {
      this.toaster.error('Usuario no autenticado.');
      return;
    }

    this.deleting = true;

    this.userProfileService.deleteMyAccount(
      this.currentUserId
    )
    .subscribe({
      next: () => {
        this.toaster.success(
          'Tu cuenta fue eliminada correctamente.',
          'Cuenta eliminada'
        );

        setTimeout(() => {
          this.authService.logout().subscribe({
            next: () => {
              this.router.navigate(['/']);
            },
            error: () => {
              window.location.href = '/';
            }
          });
        }, 1200);
      },
      error: err => {
        this.toaster.error(
          err.error?.error?.message ?? 'No fue posible eliminar la cuenta.',
          'Error'
        );
        this.deleting = false;
      }
    });
  }
}