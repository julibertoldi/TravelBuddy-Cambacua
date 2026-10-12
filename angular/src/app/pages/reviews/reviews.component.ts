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

  constructor(
    private restService: RestService,
    private configState: ConfigStateService,
    private fb: FormBuilder
  ) {
    this.reviewForm = this.fb.group({
      estrellas: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
      titulo: ['', [Validators.required, Validators.maxLength(200)]],
      comentario: ['', [Validators.required, Validators.maxLength(2000)]],
      palabrasClave: ['', [Validators.maxLength(200)]]
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
          id: d.id || d.Id,
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
