import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
  
    {
      path: '/cities',
      name: 'Ciudades',
      iconClass: 'fas fa-city',
      order: 2,
      layout: eLayoutType.application,
    },
    {
      path: '/user-profile',
      name: 'Mi Perfil',
      iconClass: 'fas fa-user',
      order: 3,
      layout: eLayoutType.application,
    },
    {
      path: '/reviews',
      name: 'Calificaciones y Reseñas',
      iconClass: 'fas fa-star',
      order: 4,
      layout: eLayoutType.application,
    },
    {
      path: '/favorites',
      name: 'Mis Favoritos',
      iconClass: 'fas fa-heart',
      order: 5,
      layout: eLayoutType.application,
    },
    {
      path: '/experiences',
      name: 'Experiencias',
      iconClass: 'fas fa-map-marked-alt',
      order: 6,
      layout: eLayoutType.application,
    },
    {
      path: '/notifications-settings',
      name: 'Configuración Notificaciones',
      iconClass: 'fas fa-bell',
      order: 7,
      layout: eLayoutType.application,
    },
    {
      path: '/admin',
      name: 'Panel Admin',
      iconClass: 'fa fa-chart-line',
      order: 10,
      layout: eLayoutType.application, 
      requiredPolicy: 'TravelBuddy.Admin', 
    }
  ]);
}