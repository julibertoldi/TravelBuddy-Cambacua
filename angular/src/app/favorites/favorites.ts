import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RestService } from '@abp/ng.core';
import { RouterLink } from '@angular/router';

import { finalize } from 'rxjs/operators';

export interface FavoriteDto {
  usuarioId: string;
  destinoId: string;
  id?: string;
  name?: string;
  nombre?: string;
  description?: string;
  region?: string;
  country?: string;
  ubicacion?: string;
  imageUrl?: string;
  imagenUrl?: string;
  price?: number;
  isAvailable?: boolean;
  geoDbCityId?: number;
  population?: number;
  latitude?: number;
  longitude?: number;
  creationTime?: string;
}

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './favorites.html',
  styleUrls: ['./favorites.scss']
})
export class FavoritesComponent implements OnInit {
  favoritos: FavoriteDto[] = [];
  loading = false;

  // Variables para controlar el Modal de Confirmación
  showConfirmModal = false;
  destinoAEliminarId: string | null = null;
  destinoAEliminarNombre: string = '';

  constructor(
    private restService: RestService
  ) { }

  ngOnInit(): void {
    this.obtenerFavoritos();
  }

  // GET - Obtiene la lista de destinos 
  obtenerFavoritos(): void {
    this.loading = true;
    this.restService.request<any, FavoriteDto[]>({
      method: 'GET',
      url: '/api/app/favorites'
    })
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (data) => {
          this.favoritos = data;
        },
        error: (err) => console.error('Error al cargar favoritos', err)
      });
  }

  confirmarEliminar(fav: FavoriteDto): void {
    const id = fav.destinoId || fav.id;
    const nombre = fav.nombre || fav.name || 'este destino';
    if (!id) {
      console.error('El destino no tiene un ID válido', fav);
      return;
    }
    this.destinoAEliminarId = id;
    this.destinoAEliminarNombre = nombre;
    this.showConfirmModal = true;
  }

  // 3. Cierra la ventana de confirmación
  cancelarEliminacion(): void {
    this.showConfirmModal = false;
    this.destinoAEliminarId = null;
    this.destinoAEliminarNombre = '';
  }

  // 4. DELETE - Elimina el destino tras confirmar
  eliminarFavorito(destinoId: string): void {
    if (!destinoId) return;

    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/favorites/quitar/${destinoId}`
    }).subscribe({
      next: () => {
        this.favoritos = this.favoritos.filter(f => (f.destinoId || f.id) !== destinoId);
        this.cancelarEliminacion();
      },
      error: (err) => {
        console.error('Error al quitar favorito', err);
        this.cancelarEliminacion();
      }
    });
  }

  // Mantiene tu función de toggle original si la usás en otras partes
  toggleFavorito(destinoId: string): void {
    const yaEsFavorito = this.favoritos.some(f => (f.destinoId || f.id) === destinoId);

    if (yaEsFavorito) {
      const favObj = this.favoritos.find(f => (f.destinoId || f.id) === destinoId);
      if (favObj) this.confirmarEliminar(favObj);
    } else {
      this.restService.request<any, any>({
        method: 'POST',
        url: `/api/app/favorites/agregar/${destinoId}`
      }).subscribe({
        next: () => this.obtenerFavoritos(),
        error: (err) => console.error('Error al añadir favorito', err)
      });
    }
  }

  esFavorito(destinoId: string): boolean {
    return this.favoritos.some(f => (f.destinoId || f.id) === destinoId);
  }
}