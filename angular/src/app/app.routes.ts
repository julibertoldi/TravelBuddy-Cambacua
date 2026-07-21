import { authGuard } from '@abp/ng.core';
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
    path: 'reviews',
    loadComponent: () => import('./pages/reviews/reviews.component').then(c => c.ReviewsComponent),
  },
  {
    path: 'favorites',
    canActivate: [authGuard], // Protege la ruta para que solo entren usuarios logueados
    loadComponent: () => import('./favorites/favorites').then(c => c.FavoritesComponent),
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