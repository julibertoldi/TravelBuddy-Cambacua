import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { PublicUserProfileDto, UpdateUserProfileDto } from '../users/models';

@Injectable({
  providedIn: 'root',
})
export class UserProfileService {
  apiName = 'Default';
  

  deleteMyAccount = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/user-profile/me',
    },
    { apiName: this.apiName,...config });
  

  getMyProfile = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: '/api/user-profile/me',
    },
    { apiName: this.apiName,...config });
  

  updateMyProfile = (input: UpdateUserProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/user-profile/me',
      params: { nombre: input.nombre, apellido: input.apellido, fotoPerfilUrl: input.fotoPerfilUrl, preferencias: input.preferencias, email: input.email },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
