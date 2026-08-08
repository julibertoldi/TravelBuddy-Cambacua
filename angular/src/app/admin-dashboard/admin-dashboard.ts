import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../proxy/admin/admin.service';
import { AdminDashboardStatsDto } from '../proxy/admin/models'; // <-- Lo importamos desde models
import { StatisticsService, ApiCallLogDto, AdminDashboardDto } from '../proxy/statistics';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.scss']
})
export class AdminDashboardComponent implements OnInit {
  // Estadísticas generales (Usuarios, Destinos, Favoritos)
  stats?: AdminDashboardStatsDto;
  
  // Estadísticas y logs de la API externa
  statisticsData?: AdminDashboardDto;
  apiLogs: ApiCallLogDto[] = [];

  constructor(
    private adminService: AdminService,
    private statisticsService: StatisticsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadGeneralStats();
    this.loadApiStatistics();
    this.loadApiLogs();
  }

  loadGeneralStats() {
    this.adminService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error al cargar estadísticas generales', err)
    });
  }

  loadApiStatistics() {
    this.statisticsService.getDashboardStatistics().subscribe({
      next: (data) => {
        this.statisticsData = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error al cargar métricas de API', err)
    });
  }

  loadApiLogs() {
    this.statisticsService.getApiCallLogs().subscribe({
      next: (data) => {
        this.apiLogs = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error al cargar los logs de la API', err)
    });
  }

  downloadCsv() {
    this.statisticsService.exportApiLogsCsv().subscribe({
      next: (response: any) => {
        const blob = new Blob([response], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'api-logs-report.csv';
        anchor.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => console.error('Error al exportar el archivo CSV', err)
    });
  }
}