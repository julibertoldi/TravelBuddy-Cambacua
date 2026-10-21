import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'cities',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/cities/cities.component').then(c => c.CitiesComponent),
  },
  {
    path: 'reviews',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/reviews/reviews.component').then(c => c.ReviewsComponent),
  },
  {
    path: 'favorites',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./favorites/favorites').then(c => c.FavoritesComponent),
  },
  {
    path: 'account',
    loadChildren: () =>
      import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () =>
      import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'user-profile',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/user-profile/user-profile.component').then(
        c => c.UserProfileComponent
      ),
  },
  {
    path: 'notifications-settings',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/notifications-settings/notifications-settings').then(
        m => m.NotificationsSettings
      ),
  },
  {
    path: 'admin',
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'TravelBuddy.Admin',
    },
    loadComponent: () =>
      import('./pages/admin/admin.component').then(c => c.AdminComponent),
  },
];