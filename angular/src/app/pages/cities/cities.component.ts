import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; 
import { ToasterService } from '@abp/ng.theme.shared'; 
import { RestService } from '@abp/ng.core';           
import { DestinationService } from '../../proxy/destinations/destination.service';

@Component({
  selector: 'app-cities',
  standalone: true, 
  imports: [
    CommonModule,         
    ReactiveFormsModule   
  ],
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.css']
})
export class CitiesComponent implements OnInit {
  // Variables para la búsqueda y estado
  searchForm!: FormGroup; 
  cities: any[] = [];
  favoriteCityIds: Set<string | number> = new Set(); // Para rastrear favoritos guardados
  loading = false;
  hasSearched: boolean = false;

  constructor(
    private fb: FormBuilder,
    private destinationService: DestinationService,
    private restService: RestService, 
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadFavorites(); // Carga previa de los favoritos del usuario
  }

  buildForm() {
    this.searchForm = this.fb.group({
      name: [''],          
      country: [''],        
      region: [''],          
      minPopulation: [null]  
    });
  }

  // Cargar la lista de favoritos del usuario al iniciar
  loadFavorites(): void {
    this.restService.request<any, any[]>({
      method: 'GET',
      url: '/api/app/favorites'
    }).subscribe({
      next: (data) => {
        if (data && Array.isArray(data)) {
          // Guarda el destinoId como el id/nombre por compatibilidad de datos de GeoDB
          const ids = data.map(f => f.destinoId || f.id || f.nombre);
          this.favoriteCityIds = new Set(ids);
        }
      },
      error: (err) => console.error('Error al cargar la lista de favoritos', err)
    });
  }

  // Validación de Filtros: Deshabilita el botón si todo está vacío o si ningún campo tiene al menos 3 caracteres
  isSearchInvalid(): boolean {
    if (!this.searchForm) return true;

    const { name, country, region, minPopulation } = this.searchForm.value;

    const nameStr = (name || '').trim();
    const countryStr = (country || '').trim();
    const regionStr = (region || '').trim();

    // Si absolutamente todo está vacío
    if (!nameStr && !countryStr && !regionStr && !minPopulation) {
      return true;
    }

    // Si ingresó algún texto, exigir que al menos un campo cumpla un mínimo de 3 caracteres
    const hasValidMinLength = nameStr.length >= 3 || countryStr.length >= 3 || regionStr.length >= 3 || minPopulation > 0;

    return !hasValidMinLength;
  }

  search() {
    if (this.isSearchInvalid()) return;

    this.loading = true;
    this.hasSearched = false;
    this.cities = [];

    this.loading = true;
    const { name, country, region } = this.searchForm.value;
    this.hasSearched = true;

    this.destinationService.searchCities({ 
      partialName: name, 
      pais: country || undefined, 
      region: region || undefined
    }).subscribe({
      next: (response: any) => {
        this.cities = response.cities || response.items || response || [];
        this.loading = false;
        this.hasSearched = true;
      },
      error: () => {
        this.toaster.error('Error al consultar la API externa.');
        this.loading = false;
        this.hasSearched = true;
      }
    });
  }
// Para consultar en el HTML si una ciudad ya está guardada
  isAlreadySaved(city: any): boolean {
    const identifier = city.id || city.wikiDataId || city.name;
    return this.favoriteCityIds.has(identifier);
  }
  // Método unificado usando restService
  saveToFavorites(city: any) {
    this.loading = true;

    // Guardar el destino en la base de datos local
    this.destinationService.importFromGeoDb(city.id).subscribe({
      next: (destinoGuardado: any) => {
        const destinoId = destinoGuardado.id || destinoGuardado;

        //  /api/app/favorites/agregar/{destinoId}
        this.restService.request<any, void>({
          method: 'POST',
          url: `/api/app/favorites/agregar/${destinoId}`
        }).subscribe({
          next: () => {
            this.toaster.success(`¡${city.name} se guardó en tus favoritos!`);
            this.loading = false;
          },
          error: (err) => {
            console.error(err);
            this.toaster.error('Error al agregar a mis favoritos.');
            this.loading = false;
          }
        });

      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Error al guardar el destino en la base de datos.');
        this.loading = false;
      }
    });
  }
}