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
  ConfirmationService
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

        this.profileForm.patchValue({
          nombre: profile.nombre,
          apellido: profile.apellido,
          email: profile.email,
          fotoPerfilUrl: profile.fotoPerfilUrl,
          preferencias: profile.preferencias
        });

        this.loading = false;
      },

      error: err => {

        this.toaster.error(
          err.error?.error?.message ?? 'No se pudo cargar el perfil.',
          'Error'
        );

        this.loading = false;
      }

    });

  }

  onSubmit(): void {

    if (this.profileForm.invalid) {
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

        setTimeout(() => {
          window.location.reload();
        }, 1000);

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

  confirmDelete(): void {

    this.confirmation.warn(
      '¿Estás seguro de eliminar tu cuenta?',
      'Eliminar cuenta'
    ).subscribe((status: any) => { 
      if (status === 1) { 
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
          'La cuenta fue eliminada.',
          'Cuenta eliminada'
        );

        setTimeout(() => {

          this.authService.logout().subscribe(() => {
            this.router.navigate(['/']);
          });

        }, 1000);

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
