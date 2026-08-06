import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AdminDashboardStatsDto } from '../admin/models';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  apiName = 'Default';
  

  getDashboardStats = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, AdminDashboardStatsDto>({
      method: 'GET',
      url: '/api/app/admin/dashboard-stats',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
