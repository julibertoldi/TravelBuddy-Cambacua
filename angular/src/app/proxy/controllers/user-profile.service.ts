import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { PublicUserProfileDto, UpdateUserProfileDto } from '../users/models';
import type { PublicUserProfileDto } from '../users/models';
import type { PublicUserProfileDto, UpdateUserProfileDto } from '../users/models';

@Injectable({
  providedIn: 'root',
})
export class UserProfileService {
  apiName = 'Default';
  

  getPublicProfile = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: `/api/user-profile/${userId}`,
  deleteMyAccount = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/user-profile/me',
  getPublicProfile = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: `/api/user-profile/${userId}`,
    },
    { apiName: this.apiName,...config });
  

  updateMyProfile = (input: UpdateUserProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/user-profile/me',
      params: { nombre: input.nombre, apellido: input.apellido, fotoPerfilUrl: input.fotoPerfilUrl, preferencias: input.preferencias },
  getPublicProfile = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: `/api/user-profile/${userId}`,
  updateMyProfile = (input: UpdateUserProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/user-profile/me',
      params: { nombre: input.nombre, apellido: input.apellido, fotoPerfilUrl: input.fotoPerfilUrl, preferencias: input.preferencias },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
