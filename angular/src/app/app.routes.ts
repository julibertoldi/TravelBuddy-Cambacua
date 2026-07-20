import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'user-profile',
    loadComponent: () => import('./pages/user-profile/user-profile.component').then(c => c.UserProfileComponent),
    canActivate: [authGuard]
  },
{
  path: 'experiences',
  loadComponent: () =>
    import('./pages/experiences/experiences.component')
      .then(c => c.ExperiencesComponent),
  canActivate: [authGuard],
},
];
