import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../proxy/admin/admin.service';
import { AdminDashboardStatsDto } from '../../proxy/admin/models';
import {
  AdminDashboardDto,
  ApiCallLogDto,
  AuditLogExceptionDto,
  StatisticsService,
} from '../../proxy/statistics';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.component.html',
})
export class AdminComponent implements OnInit {
  stats?: AdminDashboardStatsDto;
  dashboard?: AdminDashboardDto;
  apiLogs: ApiCallLogDto[] = [];
  internalErrors: AuditLogExceptionDto[] = [];

  constructor(
    private adminService: AdminService,
    private statisticsService: StatisticsService
  ) {}

  ngOnInit(): void {
    this.adminService.getDashboardStats().subscribe({
      next: data => {
        this.stats = data;
      },
      error: error => console.error('Error al cargar estadísticas administrativas', error),
    });
    this.statisticsService.getDashboardStatistics().subscribe({
      next: data => {
        this.dashboard = data;
      },
      error: error => console.error('Error al cargar métricas', error),
    });
    this.statisticsService.getApiCallLogs().subscribe({
      next: data => {
        this.apiLogs = data;
      },
      error: error => console.error('Error al cargar fallos de API', error),
    });
    this.statisticsService.getInternalErrorLogs().subscribe({
      next: data => {
        this.internalErrors = data;
      },
      error: error => console.error('Error al cargar excepciones internas', error),
    });
  }

  downloadCsv(): void {
    this.statisticsService.exportApiLogsCsv().subscribe({
      next: (response: Blob) => {
        const url = window.URL.createObjectURL(response);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'api-logs-report.csv';
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: error => console.error('Error al exportar métricas CSV', error),
    });
  }
}