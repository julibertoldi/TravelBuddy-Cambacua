import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { FavoriteDto } from '../favorites/models';

@Injectable({
  providedIn: 'root',
})
export class FavoriteService {
  apiName = 'Default';
  

  agregarFavorito = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/favorites/agregar/${destinoId}`,
    },
    { apiName: this.apiName,...config });
  

  obtenerMisFavoritos = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FavoriteDto[]>({
      method: 'GET',
      url: '/api/app/favorites',
    },
    { apiName: this.apiName,...config });
  

  quitarFavorito = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/favorites/quitar/${destinoId}`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
