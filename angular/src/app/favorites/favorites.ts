import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RestService } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';

export interface FavoriteDto {
  id: string;
  destinoId: string;
  nombreDestino?: string; 
  imagenUrl?: string; 
}

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './favorites.html',
  styleUrls: ['./favorites.scss']
})
export class FavoritesComponent implements OnInit {
  favoritos: FavoriteDto[] = [];
  loading = false;

  constructor(private restService: RestService) {}

  ngOnInit(): void {
    this.obtenerFavoritos();
  }

  // 1. GET - Obtiene la lista de destinos 
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

  // 2. POST - Agrega un nuevo destino 
  agregarDestinoPrueba(): void {
    const destinoCanabacuaId = '053ad7de-baf1-55de-dec4-3a1d69591b3e';
    
    this.restService.request<any, any>({
      method: 'POST',
      url: `/api/app/favorites/agregar/${destinoCanabacuaId}`
    }).subscribe({
      next: () => {
        this.obtenerFavoritos();
      },
      error: (err) => console.error('Error al añadir favorito', err)
    });
  }

  // 3. DELETE - Elimina el destino 
  eliminarFavorito(destinoId: string): void {
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/favorites/quitar/${destinoId}`
    }).subscribe({
      next: () => {
        this.favoritos = this.favoritos.filter(f => f.destinoId !== destinoId);
      },
      error: (err) => console.error('Error al quitar favorito', err)
    });
  }
  // Función de tipo "Toggle" para alternar entre agregar y quitar
toggleFavorito(destinoId: string): void {
  const yaEsFavorito = this.favoritos.some(f => f.destinoId === destinoId);

  if (yaEsFavorito) {
    // Si ya está, lo quitamos
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/favorites/quitar/${destinoId}`
    }).subscribe({
      next: () => {
        this.favoritos = this.favoritos.filter(f => f.destinoId !== destinoId);
      },
      error: (err) => console.error('Error al quitar favorito', err)
    });
  } else {
    // Si no está, lo agregamos
    this.restService.request<any, any>({
      method: 'POST',
      url: `/api/app/favorites/agregar/${destinoId}`
    }).subscribe({
      next: () => {
        this.obtenerFavoritos(); // Refrescamos la lista para traer el DTO completo
      },
      error: (err) => console.error('Error al añadir favorito', err)
    });
  }
}

// Método auxiliar para saber qué corazón pintar en el HTML
esFavorito(destinoId: string): boolean {
  return this.favoritos.some(f => f.destinoId === destinoId);
}
}
