import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../proxy/admin/admin.service';
import { AdminDashboardStatsDto } from '../../proxy/admin/models';
import { StatisticsService, ApiCallLogDto, AdminDashboardDto } from '../../proxy/statistics';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Panel de Administración - TravelBuddy</h2>
        <button class="btn btn-success" (click)="downloadCsv()">Exportar Logs CSV</button>
      </div>
      <hr>

      <!-- Fila 1: Estadísticas Generales -->
      <div class="row mb-4" *ngIf="stats">
        <div class="col-md-3"><div class="card bg-primary text-white p-3"><h5>Usuarios</h5><h3>{{ stats.totalUsers }}</h3></div></div>
        <div class="col-md-3"><div class="card bg-success text-white p-3"><h5>Destinos</h5><h3>{{ stats.totalDestinations }}</h3></div></div>
        <div class="col-md-3"><div class="card bg-info text-white p-3"><h5>Favoritos</h5><h3>{{ stats.totalFavorites }}</h3></div></div>
        <div class="col-md-3"><div class="card bg-danger text-white p-3"><h5>Errores API</h5><h3>{{ stats.externalApiErrorsCount }}</h3></div></div>
      </div>

      <!-- Fila 2: Tabla de Logs -->
      <div class="card shadow-sm">
        <div class="card-header bg-dark text-white">Bitácora de API</div>
        <table class="table table-striped mb-0">
          <thead>
            <tr><th>Fecha</th><th>Endpoint</th><th>Status</th><th>Éxito</th></tr>
          </thead>
          <tbody>
            <tr *ngFor="let log of apiLogs">
              <td>{{ log.timestamp | date:'short' }}</td>
              <td><code>{{ log.endpoint }}</code></td>
              <td><span class="badge" [ngClass]="log.statusCode === 200 ? 'bg-success' : 'bg-danger'">{{ log.statusCode }}</span></td>
              <td>{{ log.isSuccess ? 'Sí' : 'No' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class AdminComponent implements OnInit {
  stats?: AdminDashboardStatsDto;
  apiLogs: ApiCallLogDto[] = [];

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
    this.statisticsService.getApiCallLogs().subscribe(data => { 
        this.apiLogs = data; 
        this.cdr.detectChanges(); 
    });
  }

  downloadCsv() {
    this.statisticsService.exportApiLogsCsv().subscribe((response: any) => {
      const blob = new Blob([response], { type: 'text/csv' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'api-logs-report.csv';
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}