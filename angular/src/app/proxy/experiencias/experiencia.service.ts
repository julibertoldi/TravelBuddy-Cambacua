import { Injectable } from '@angular/core';
import { RestService, Rest, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';
import {
  CreateUpdateExperienciaDto,
  ExperienciaDto,
  ExperienciaGetListInput,
} from './models';

@Injectable({
  providedIn: 'root',
})
export class ExperienciaService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getList = (
    input: ExperienciaGetListInput,
    config?: Partial<Rest.Config>
  ): Observable<PagedResultDto<ExperienciaDto>> =>
    this.restService.request<any, PagedResultDto<ExperienciaDto>>(
      {
        method: 'GET',
        url: '/api/app/experiencia',
        params: {
          destinoId: input.destinoId,
          valoracion: input.valoracion,
          keyword: input.keyword,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  get = (
    id: string,
    config?: Partial<Rest.Config>
  ): Observable<ExperienciaDto> =>
    this.restService.request<any, ExperienciaDto>(
      {
        method: 'GET',
        url: `/api/app/experiencia/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  create = (
    input: CreateUpdateExperienciaDto,
    config?: Partial<Rest.Config>
  ): Observable<ExperienciaDto> =>
    this.restService.request<any, ExperienciaDto>(
      {
        method: 'POST',
        url: '/api/app/experiencia',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  update = (
    id: string,
    input: CreateUpdateExperienciaDto,
    config?: Partial<Rest.Config>
  ): Observable<ExperienciaDto> =>
    this.restService.request<any, ExperienciaDto>(
      {
        method: 'PUT',
        url: `/api/app/experiencia/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  delete = (
    id: string,
    config?: Partial<Rest.Config>
  ): Observable<void> =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/app/experiencia/${id}`,
      },
      { apiName: this.apiName, ...config }
    );
}