import type { PublicUserProfileDto, UpdateUserProfileDto, UserProfileDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UserProfileService {
  apiName = 'Default';
  

  getMyProfile = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserProfileDto>({
      method: 'GET',
      url: '/api/app/user-profile/my-profile',
    },
    { apiName: this.apiName,...config });
  

  getPublicProfile = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: `/api/app/user-profile/public-profile/${userId}`,
    },
    { apiName: this.apiName,...config });
  

  updateMyProfile = (input: UpdateUserProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/user-profile/my-profile',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
