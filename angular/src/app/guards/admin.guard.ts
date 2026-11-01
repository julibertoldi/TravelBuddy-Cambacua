import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';

export const adminGuard: CanActivateFn = () => {
  const permissionService = inject(PermissionService);
  const router = inject(Router);

  return permissionService.getGrantedPolicy('TravelBuddy.Admin')
    ? true
    : router.createUrlTree(['/']);
};