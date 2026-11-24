import type { AdminDashboardDto, ApiCallLogDto, AuditEventDto, AuditLogExceptionDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StatisticsService {
  apiName = 'Default';
  

  exportApiLogsCsv = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/statistics/export-api-logs-csv',
    },
    { apiName: this.apiName,...config });
  

  getApiCallLogs = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApiCallLogDto[]>({
      method: 'GET',
      url: '/api/app/statistics/api-call-logs',
    },
    { apiName: this.apiName,...config });

  getInternalErrorLogs = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, AuditLogExceptionDto[]>({
      method: 'GET',
      url: '/api/app/statistics/internal-error-logs',
    },
    { apiName: this.apiName,...config });

  getCriticalAuditEvents = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, AuditEventDto[]>({
      method: 'GET',
      url: '/api/app/statistics/critical-audit-events',
    },
    { apiName: this.apiName,...config });
  

  getDashboardStatistics = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, AdminDashboardDto>({
      method: 'GET',
      url: '/api/app/statistics/dashboard-statistics',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
