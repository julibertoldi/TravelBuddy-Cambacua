import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RestService, ConfigStateService } from '@abp/ng.core';
import { ActivatedRoute } from '@angular/router';
import { FavoriteService } from '../../proxy/controllers/favorite.service';

interface Destination {
  id: string;
  nombre: string;
  descripcion: string;
  ubicacion: string;
  precio: number;
  imagenUrl: string;
  disponible: boolean;
}

interface Review {
  id: string;
  destinoId: string;
  usuarioId: string;
  estrellas: number;
  titulo: string;
  comentario: string;
  palabrasClave: string;
  creationTime: string;
}

interface Promedio {
  promedio: number;
  totalCalificaciones: number;
}

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './reviews.component.html',
  styleUrls: ['./reviews.component.scss']
})
export class ReviewsComponent implements OnInit {
  destinations: Destination[] = [];
  selectedDestination: Destination | null = null;
  reviews: Review[] = [];
  promedio: Promedio = { promedio: 0, totalCalificaciones: 0 };
  
  reviewForm: FormGroup;
  isEditing = false;
  editingReviewId: string | null = null;
  currentUserId: string | null = null;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // Rating input state
  hoveredStars = 0;
  selectedStars = 0;
  private requestedDestinationId: string | null = null;

  constructor(
    private restService: RestService,
    private configState: ConfigStateService,
    private favoriteService: FavoriteService,
    private fb: FormBuilder,
    private route: ActivatedRoute
  ) {
    this.reviewForm = this.fb.group({
      estrellas: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
      titulo: ['', [Validators.required, Validators.maxLength(200)]],
      comentario: ['', [Validators.required, Validators.maxLength(2000)]],
      palabrasClave: ['', [Validators.maxLength(200)]]
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.requestedDestinationId = params.get('destinoId');
      this.selectRequestedDestination();
    });
    this.loadDestinations();
    this.currentUserId = this.getCurrentUserId();
  }

  getCurrentUserId(): string | null {
    const currentUser = this.configState.getOne('currentUser');
    return currentUser?.id ?? null;
  }

  loadDestinations(): void {
    this.favoriteService.obtenerMisFavoritos().subscribe({
      next: (favorites) => {
        this.errorMessage = null;
        this.destinations = favorites.map((favorite) => ({
          id: favorite.destinoId ?? '',
          nombre: favorite.name ?? 'Sin nombre',
          descripcion: favorite.description ?? '',
          ubicacion: favorite.region || favorite.country || 'Ubicación no especificada',
          precio: favorite.price ?? 0,
          imagenUrl: favorite.imageUrl ?? '',
          disponible: favorite.isAvailable ?? false
        }));

        if (this.destinations.length > 0) {
          const requestedDestination = this.destinations.find(
            destination => destination.id === this.requestedDestinationId
          );
          this.selectDestination(requestedDestination ?? this.destinations[0]);
        }
      },
      error: (err) => {
        this.errorMessage = 'Error al cargar tus favoritos.';
      }
    });
  }

  private selectRequestedDestination(): void {
    if (!this.requestedDestinationId) {
      return;
    }

    const destination = this.destinations.find(
      item => item.id === this.requestedDestinationId
    );
    if (destination && destination.id !== this.selectedDestination?.id) {
      this.selectDestination(destination);
    }
  }

  selectDestination(destination: Destination): void {
    this.selectedDestination = destination;
    this.cancelEdit();
    this.errorMessage = null;
    this.successMessage = null;

    if (destination) {
      this.loadReviewsAndPromedio(destination.id);
    }
  }

  loadReviewsAndPromedio(destinoId: string): void {
    // Load reviews with casing normalization
    this.restService.request<any, any>({
      method: 'GET',
      url: `/api/app/calificacion?destinoId=${destinoId}`
    }).subscribe({
      next: (response) => {
        const rawItems = response.items || response;
        this.reviews = rawItems.map((r: any) => ({
          id: r.id || r.Id,
          destinoId: r.destinoId || r.DestinoId,
          usuarioId: r.usuarioId || r.UsuarioId || r.userId || r.UserId,
          estrellas: r.estrellas ?? r.Estrellas ?? 0,
          titulo: r.titulo || r.Titulo || '',
          comentario: r.comentario || r.Comentario || '',
          palabrasClave: r.palabrasClave || r.PalabrasClave || '',
          creationTime: r.creationTime || r.CreationTime || ''
        }));
      },
      error: (err) => {
        this.errorMessage = 'Error al cargar las reseñas.';
      }
    });

    // Load average
    this.restService.request<Promedio, any>({
      method: 'GET',
      url: '/api/app/calificaciones-custom/promedio-por-destino',
      params: { destinoId: destinoId }
    }).subscribe({
      next: (response) => {
        this.promedio = response;
      },
      error: (err) => {
        console.error('Error loading promedio:', err);
      }
    });
  }
  
  setStars(stars: number): void {
    this.selectedStars = stars;
    this.reviewForm.patchValue({ estrellas: stars });
  }

  hoverStars(stars: number): void {
    this.hoveredStars = stars;
  }

  clearHover(): void {
    this.hoveredStars = 0;
  }

  getStarArray(count: number): number[] {
    return Array(count).fill(0);
  }

  getEmptyStarArray(rating: number): number[] {
    const emptyCount = Math.max(0, 5 - Math.round(rating));
    return Array(emptyCount).fill(0);
  }

  onSubmit(): void {
    if (this.reviewForm.invalid || !this.selectedDestination) {
      return;
    }

    const { estrellas, titulo, comentario, palabrasClave } = this.reviewForm.value;
    const destinoId = this.selectedDestination.id;

    if (this.isEditing && this.editingReviewId) {
      // Actualizar registro existente seleccionado
      const body = { destinoId, estrellas, titulo, comentario, palabrasClave };
      this.restService.request<any, any>({
        method: 'PUT',
        url: `/api/app/calificacion/${this.editingReviewId}`,
        body
      }).subscribe({
        next: () => {
          this.successMessage = 'Entrada actualizada correctamente.';
          this.errorMessage = null;
          this.cancelEdit();
          this.loadReviewsAndPromedio(destinoId);
        },
        error: (err) => {
          this.errorMessage = err.error?.error?.message || 'Error al actualizar la entrada.';
        }
      });
    } else {
      // Crear nueva entrada en la bitácora
      const body = { destinoId, estrellas, titulo, comentario, palabrasClave };
      this.restService.request<any, any>({
        method: 'POST',
        url: '/api/app/calificacion',
        body
      }).subscribe({
        next: () => {
          this.successMessage = 'Nueva entrada agregada a tu bitácora.';
          this.errorMessage = null;
          this.cancelEdit();
          this.loadReviewsAndPromedio(destinoId);
        },
        error: (err) => {
          this.errorMessage = err.error?.error?.message || 'Error al guardar la entrada.';
        }
      });
    }
  }

  editReview(review: Review): void {
    this.isEditing = true;
    this.editingReviewId = review.id;
    this.selectedStars = review.estrellas; 
    this.reviewForm.setValue({
      estrellas: review.estrellas,
      titulo: review.titulo,
      comentario: review.comentario,
      palabrasClave: review.palabrasClave || ''
    });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editingReviewId = null;
    this.selectedStars = 0;
    this.reviewForm.reset({ estrellas: 0, titulo: '', comentario: '', palabrasClave: '' });
  }

  deleteReview(reviewId: string): void {
    if (!reviewId) {
      this.errorMessage = 'No se encontró el ID de la reseña para eliminar.';
      return;
    }

    if (!confirm('¿Estás seguro de que quieres eliminar esta entrada?')) {
      return;
    }

    this.restService.request<any, any>({
      method: 'DELETE',
      url: `/api/app/calificacion/${reviewId}`
    }).subscribe({
      next: () => {
        this.successMessage = 'Entrada eliminada correctamente.';
        this.errorMessage = null;
        if (this.selectedDestination) {
          this.loadReviewsAndPromedio(this.selectedDestination.id);
        }
      },
      error: (err) => {
        this.errorMessage = 'Error al eliminar la entrada.';
      }
    });
  }

  getMyReviewId(): string {
    const myReview = this.reviews.find(r => r.usuarioId === this.currentUserId);
    return myReview ? myReview.id : '';
  }

  hasReviews(): boolean {
    return this.reviews && this.reviews.length > 0;
  }

  hasReviewed(): boolean {
    if (!this.currentUserId || !this.reviews) {
      return false;
    }
    return this.reviews.some(r => r.usuarioId === this.currentUserId);
  }

  startEditingMyReview(): void {
    const myReview = this.reviews.find(r => r.usuarioId === this.currentUserId);
    if (myReview) {
      this.editReview(myReview); 
    } else {
      this.isEditing = true;
    }
  }
    startNewReview(): void {
    this.isEditing = false;
    this.editingReviewId = null;
    this.selectedStars = 0;
    this.reviewForm.reset({ estrellas: 0, titulo: '', comentario: '', palabrasClave: '' });
  }
}
