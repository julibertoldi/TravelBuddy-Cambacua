import { DOCUMENT } from '@angular/common';
import { ToasterService } from '@abp/ng.theme.shared';
import {
  eIdentityComponents,
  provideIdentity,
  type IdentityConfigOptions,
} from '@abp/ng.identity';
import {
  EntityAction,
  type CreateFormPropContributorCallback,
  type EditFormPropContributorCallback,
  type EntityActionContributorCallback,
} from '@abp/ng.components/extensible';
import type { IdentityUserDto } from '../proxy/volo/abp/identity/models';
import { ProfilePictureApiService } from '../pages/user-profile/profile-picture-api.service';
import { validateProfilePictureFile } from '../pages/user-profile/profile-picture-validation';

const hideCreateProfilePictureUrl: CreateFormPropContributorCallback<IdentityUserDto> =
  formProps => {
    formProps.dropByValue('ProfilePictureUrl', prop =>
      prop.name.replace(/[^a-z]/gi, '').toLowerCase().endsWith('profilepictureurl'));
  };

const hideEditProfilePictureUrl: EditFormPropContributorCallback<IdentityUserDto> =
  formProps => {
    formProps.dropByValue('ProfilePictureUrl', prop =>
      prop.name.replace(/[^a-z]/gi, '').toLowerCase().endsWith('profilepictureurl'));
  };

const uploadProfilePictureAction: EntityActionContributorCallback<IdentityUserDto> =
  actionList => {
    actionList.addTail(
      EntityAction.create({
        text: 'Subir foto',
        permission: 'AbpIdentity.Users.Update',
        action: data => {
          const userId = data.record.id;
          if (!userId) {
            return;
          }

          const document = data.getInjected(DOCUMENT);
          const input = document.createElement('input');
          input.type = 'file';
          input.accept = 'image/jpeg,image/png,image/webp,.jpg,.jpeg,.png,.webp';
          input.style.display = 'none';
          document.body.appendChild(input);

          const cleanup = () => input.remove();
          input.addEventListener('cancel', cleanup, { once: true });
          input.addEventListener('change', () => {
            cleanup();
            const file = input.files?.[0];
            if (!file) {
              return;
            }

            const toaster = data.getInjected(ToasterService);
            const validationError = validateProfilePictureFile(file);
            if (validationError) {
              toaster.error(validationError, 'Error');
              return;
            }

            data.getInjected(ProfilePictureApiService)
              .uploadUserProfilePicture(userId, file)
              .subscribe({
                next: () => toaster.success('Foto de perfil actualizada.', 'Éxito'),
                error: error => toaster.error(
                  error.error?.error?.message ?? 'No se pudo subir la foto.',
                  'Error',
                ),
              });
          }, { once: true });

          input.click();
        },
      }),
    );
  };

export const identityProfilePictureOptions: IdentityConfigOptions = {
  entityActionContributors: {
    [eIdentityComponents.Users]: [uploadProfilePictureAction],
  },
  createFormPropContributors: {
    [eIdentityComponents.Users]: [hideCreateProfilePictureUrl],
  },
  editFormPropContributors: {
    [eIdentityComponents.Users]: [hideEditProfilePictureUrl],
  },
};

