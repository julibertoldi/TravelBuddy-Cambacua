import type { PublicUserProfileDto, UpdateUserProfileDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UserProfileService {

  apiName = 'Default';

  constructor(private restService: RestService) {}

  getMyProfile = (config?: Partial<Rest.Config>) =>
  deleteMyAccount = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/user-profile/my-account/${userId}`,
    },
    { apiName: this.apiName,...config });
  

  getPublicProfile = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: '/api/user-profile/me',
    }, {
      apiName: this.apiName,
      ...config,
    });
      url: `/api/app/user-profile/public-profile/${userId}`,
    },
    { apiName: this.apiName,...config });

  updateMyProfile = (
    input: UpdateUserProfileDto,
    config?: Partial<Rest.Config>
  ) =>

  updateMyProfile = (userId: string, input: UpdateUserProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/user-profile/me',
      url: `/api/app/user-profile/my-profile/${userId}`,
      body: input,
    }, {
      apiName: this.apiName,
      ...config,
    });

  deleteMyAccount = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/user-profile/me',
    }, {
      apiName: this.apiName,
      ...config,
    });

  getPublicProfile = (
    userId: string,
    config?: Partial<Rest.Config>
  ) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: `/api/user-profile/${userId}`,
    }, {
      apiName: this.apiName,
      ...config,
    });
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}