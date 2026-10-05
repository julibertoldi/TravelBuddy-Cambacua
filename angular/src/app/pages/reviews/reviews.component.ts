import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RestService, ConfigStateService } from '@abp/ng.core';

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
  comentario: string;
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

  constructor(
    private restService: RestService,
    private configState: ConfigStateService,
    private fb: FormBuilder
  ) {
    this.reviewForm = this.fb.group({
      estrellas: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
      comentario: ['', [Validators.required, Validators.maxLength(2000)]]
    });
  }

  ngOnInit(): void {
    this.loadDestinations();
    this.currentUserId = this.getCurrentUserId();
  }

  getCurrentUserId(): string | null {
    const currentUser = this.configState.getOne('currentUser');
    return currentUser?.id ?? null;
  }

loadDestinations(): void {
    this.restService.request<any, any>({
      method: 'GET',
      url: '/api/app/destination'
    }).subscribe({
      next: (response) => {
        const rawItems = response.items || response;
        
        this.destinations = rawItems.map((d: any) => ({
          id: d.id,
          // Probamos exhaustivamente todas las variantes posibles (español/inglés, mayúsculas/minúsculas)
          nombre: d.nombre || d.Name || d.name || 'Sin nombre',
          descripcion: d.descripcion || d.Description || d.description || '',
          ubicacion: d.ubicacion || d.Region || d.region || d.Country || d.country || 'Ubicación no especificada',
          precio: d.precio ?? d.Price ?? d.price ?? 0,
          imagenUrl: d.imagenUrl || d.ImageUrl || d.imageUrl || '',
          disponible: d.disponible ?? d.IsAvailable ?? d.isAvailable ?? false
        }));

        if (this.destinations.length > 0) {
          this.selectDestination(this.destinations[0]);
        }
      },
      error: (err) => {
        this.errorMessage = 'Error al cargar los destinos.';
      }
    });
  }

  selectDestination(destination: Destination): void {
    this.selectedDestination = destination;
    this.isEditing = false;
    this.editingReviewId = null;
    this.selectedStars = 0;
    this.hoveredStars = 0;
    this.reviewForm.reset({ estrellas: 0, comentario: '' });
    this.errorMessage = null;
    this.successMessage = null;

    if (destination) {
      this.loadReviewsAndPromedio(destination.id);
    }
  }

  loadReviewsAndPromedio(destinoId: string): void {
    // Load reviews
    this.restService.request<any, any>({
      method: 'GET',
      url: `/api/app/calificacion?destinoId=${destinoId}`
    }).subscribe({
      next: (response) => {
        this.reviews = response.items || response;
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

    const { estrellas, comentario } = this.reviewForm.value;
    const destinoId = this.selectedDestination.id;

    const existingReview = this.reviews.find(r => r.usuarioId === this.currentUserId);
    
    const reviewIdToUpdate = this.editingReviewId || existingReview?.id;

    if (reviewIdToUpdate) {

      const body = { destinoId, estrellas, comentario };
      this.restService.request<any, any>({
        method: 'PUT',
        url: `/api/app/calificacion/${reviewIdToUpdate}`,
        body
      }).subscribe({
        next: () => {
          this.successMessage = 'Reseña actualizada correctamente.';
          this.errorMessage = null;
          this.isEditing = false;
          this.editingReviewId = null;
          this.reviewForm.reset({ estrellas: 0, comentario: '' });
          this.selectedStars = 0;
          this.loadReviewsAndPromedio(destinoId);
        },
        error: (err) => {
          this.errorMessage = err.error?.error?.message || 'Error al actualizar la reseña.';
        }
      });
    } else {

      const body = { destinoId, estrellas, comentario };
      this.restService.request<any, any>({
        method: 'POST',
        url: '/api/app/calificacion',
        body
      }).subscribe({
        next: () => {
          this.successMessage = 'Reseña creada correctamente.';
          this.errorMessage = null;
          this.reviewForm.reset({ estrellas: 0, comentario: '' });
          this.selectedStars = 0;
          this.loadReviewsAndPromedio(destinoId);
        },
        error: (err) => {
          this.errorMessage = err.error?.error?.message || 'Error al enviar la reseña.';
        }
      });
    }
  }

editReview(review: Review): void {
    this.isEditing = true;
    this.editingReviewId = review.id;
    this.selectedStars = review.estrellas || review.estrellas; 
    this.reviewForm.setValue({
      estrellas: review.estrellas || review.estrellas,
      comentario: review.comentario
    });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editingReviewId = null;
    this.selectedStars = 0;
    this.reviewForm.reset({ estrellas: 0, comentario: '' });
  }

  deleteReview(reviewId: string): void {
    if (!confirm('¿Estás seguro de que quieres eliminar esta reseña?')) {
      return;
    }

    this.restService.request<any, any>({
      method: 'DELETE',
      url: `/api/app/calificacion/${reviewId}`
    }).subscribe({
      next: () => {
        this.successMessage = 'Reseña eliminada correctamente.';
        this.errorMessage = null;
        if (this.selectedDestination) {
          this.loadReviewsAndPromedio(this.selectedDestination.id);
        }
      },
      error: (err) => {
        this.errorMessage = 'Error al eliminar la reseña.';
      }
    });
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
}
