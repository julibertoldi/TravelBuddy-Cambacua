import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, AdminDashboardStatsDto } from '../../proxy/admin/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid py-4">
      <h2>Panel de Administración - TravelBuddy</h2>
      <hr>
      
      <div class="row" *ngIf="stats; else loading">
        <div class="col-md-3 mb-3">
          <div class="card bg-primary text-white p-3 shadow-sm">
            <h5>Usuarios Totales</h5>
            <h3>{{ stats.totalUsers }}</h3>
          </div>
        </div>

        <div class="col-md-3 mb-3">
          <div class="card bg-success text-white p-3 shadow-sm">
            <h5>Destinos</h5>
            <h3>{{ stats.totalDestinations }}</h3>
          </div>
        </div>

        <div class="col-md-3 mb-3">
          <div class="card bg-info text-white p-3 shadow-sm">
            <h5>Favoritos</h5>
            <h3>{{ stats.totalFavorites }}</h3>
          </div>
        </div>

        <div class="col-md-3 mb-3">
          <div class="card bg-danger text-white p-3 shadow-sm">
            <h5>Errores API Externa</h5>
            <h3>{{ stats.externalApiErrorsCount }}</h3>
          </div>
        </div>
      </div>

      <ng-template #loading>
        <div class="text-center py-5">
          <div class="spinner-border text-primary" role="status"></div>
          <p class="mt-2">Cargando estadísticas...</p>
        </div>
      </ng-template>
    </div>
  `
})
export class AdminComponent implements OnInit {
  stats?: AdminDashboardStatsDto;

  constructor(
    private adminService: AdminService,
    private cdr: ChangeDetectorRef // <-- Inyectamos ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.adminService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.cdr.detectChanges(); // <-- Forzamos a Angular a redibujar la vista al recibir los datos
      },
      error: (err) => {
        console.error('Error al cargar las estadísticas del dashboard', err);
      }
    });
  }
}