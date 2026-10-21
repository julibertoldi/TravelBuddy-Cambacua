import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CalificacionPromedioDto } from '../calificaciones/models';

@Injectable({
  providedIn: 'root',
})
export class CalificacionService {
  apiName = 'Default';
  

  getPromedioByDestino = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CalificacionPromedioDto>({
      method: 'GET',
      url: '/api/app/calificaciones-custom/promedio-por-destino',
      params: { destinoId },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
