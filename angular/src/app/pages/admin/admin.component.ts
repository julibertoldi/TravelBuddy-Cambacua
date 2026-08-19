import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../proxy/admin/admin.service';
import { AdminDashboardStatsDto } from '../../proxy/admin/models';
import { StatisticsService, ApiCallLogDto, AdminDashboardDto } from '../../proxy/statistics';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.component.html',
})
export class AdminComponent implements OnInit {
  stats?: AdminDashboardStatsDto;
  apiLogs: ApiCallLogDto[] = [];
  dashboardStats?: AdminDashboardDto;

  constructor(
    private adminService: AdminService,
    private statisticsService: StatisticsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.adminService.getDashboardStats().subscribe(data => {
      this.stats = data;
      this.cdr.detectChanges();
    });
    this.statisticsService.getApiCallLogs().subscribe({
      next: data => {
        this.apiLogs = data;
        this.cdr.detectChanges();
      },
      error: err => {
        console.warn('No se pudieron cargar los logs de API:', err);
        this.apiLogs = [];
        this.cdr.detectChanges();
      }
    });
    this.statisticsService.getDashboardStatistics().subscribe({
      next: data => {
        this.dashboardStats = data;
        this.cdr.detectChanges();
      },
      error: err => {
        console.warn('No se pudieron cargar las estadísticas del dashboard:', err);
        this.cdr.detectChanges();
      }
    });
  }

  /** Exporta estadísticas de búsqueda → Estadisticas.csv */
  exportarEstadisticas() {
    this.statisticsService.exportSearchLogsToCsv().subscribe({
      next: (response: any) => {
        const blob = new Blob([response], { type: 'text/csv;charset=utf-8;' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Estadisticas.csv';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: err => console.error('Error al exportar estadísticas:', err)
    });
  }

  /** Exporta logs de llamadas API → api-logs-report.csv */
  downloadCsv() {
    this.statisticsService.exportApiLogsCsv().subscribe({
      next: (response: any) => {
        const blob = new Blob([response], { type: 'text/csv;charset=utf-8;' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'api-logs-report.csv';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: err => console.error('Error al exportar logs:', err)
    });
  }
}