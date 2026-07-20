import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';
import { CitiesComponent } from './pages/cities/cities.component'; 

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  // NUEVA RUTA AGREGADA
{
  path: 'cities',
  loadComponent: () => import('./pages/cities/cities.component').then(c => c.CitiesComponent),
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
];
