import type { FavoriteDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FavoriteService {
  apiName = 'Default';
  

  agregarDesdeGeoDb = (geoDbCityId: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/favorite/agregar-desde-geo-db/${geoDbCityId}`,
    },
    { apiName: this.apiName,...config });
  

  agregarFavorito = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/favorite/agregar-favorito/${destinoId}`,
    },
    { apiName: this.apiName,...config });
  

  obtenerMisFavoritos = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FavoriteDto[]>({
      method: 'POST',
      url: '/api/app/favorite/obtener-mis-favoritos',
    },
    { apiName: this.apiName,...config });
  

  quitarFavorito = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/favorite/quitar-favorito/${destinoId}`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
