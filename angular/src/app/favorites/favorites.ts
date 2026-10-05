import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
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
  imports: [CommonModule, RouterLink, FormsModule], 
  templateUrl: './favorites.html',
  styleUrls: ['./favorites.scss']
})
export class FavoritesComponent implements OnInit {
  favoritos: FavoriteDto[] = [];
  loading = false;

  // Variable de filtro y modal
  searchTerm: string = '';
  showConfirmModal = false;
  destinoAEliminarId: string | null = null;
  destinoAEliminarNombre: string = '';

  constructor(private restService: RestService) { }

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

  // Getter mejorado para filtrado dinámico flexible (nombre, región, país o ubicación)
  get favoritosFiltrados(): FavoriteDto[] {
    if (!this.searchTerm || this.searchTerm.trim() === '') {
      return this.favoritos;
    }

    const busqueda = this.searchTerm.toLowerCase().trim();

    return this.favoritos.filter(fav => {
      const nombre = (fav.name || fav.nombre || '').toLowerCase();
      const pais = (fav.country || fav.ubicacion || '').toLowerCase();
      const region = (fav.region || '').toLowerCase();

      return nombre.includes(busqueda) || pais.includes(busqueda) || region.includes(busqueda);
    });
  }
onSearchChange(): void {
    // Permite refrescar la lista filtrada al tipear en el input
  }

  limpiarBusqueda(): void {
    this.searchTerm = '';
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

  cancelarEliminacion(): void {
    this.showConfirmModal = false;
    this.destinoAEliminarId = null;
    this.destinoAEliminarNombre = '';
  }

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