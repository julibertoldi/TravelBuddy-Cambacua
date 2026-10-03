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

  searchForm!: FormGroup; 
  cities: any[] = [];
  featuredCities: any[] = []; // Lista para las ciudades de muestra al cargar
  favoriteCityIds: Set<string | number> = new Set();
  loading = false;
  loadingFeatured = false;
  hasSearched: boolean = false;

  constructor(
    private fb: FormBuilder,
    private destinationService: DestinationService,
    private restService: RestService, 
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadFavorites();
    this.loadFeaturedCities(); // Cargar ciudades destacadas/muestra
  }

  buildForm() {
    this.searchForm = this.fb.group({
      name: [''],          
      country: [''],        
      region: [''],          
      minPopulation: [null]  
    });
  }
// Cargar ciudades destacadas/de muestra al iniciar
loadFeaturedCities(): void {
  this.loadingFeatured = true;

  // Forzamos la búsqueda inicial indicando el país "Argentina"
  this.destinationService.searchCities({ 
    partialName: 'c',
    pais: 'AR'  
  }).subscribe({
    next: (response: any) => {
      const results = response.cities || response.items || response || [];
      this.featuredCities = results.slice(0, 6); // Toma las primeras 6 ciudades de Argentina
      this.loadingFeatured = false;
    },
    error: () => {
      this.loadingFeatured = false;
    }
  });
}

  // Carga de favoritos normalizando IDs y Nombres en minúsculas
  loadFavorites(): void {
    this.restService.request<any, any[]>({
      method: 'GET',
      url: '/api/app/favorites'
    }).subscribe({
      next: (data) => {
        if (data && Array.isArray(data)) {
          const identifiers = data.flatMap(f => [
            f.destinoId,
            f.id,
            (f.nombre || f.name || f.ubicacion || '').toLowerCase().trim()
          ]).filter(Boolean);

          this.favoriteCityIds = new Set(identifiers);
        }
      },
      error: (err) => console.error('Error al cargar la lista de favoritos', err)
    });
  }

  // Validación de Filtros
  isSearchInvalid(): boolean {
    if (!this.searchForm) return true;

    const { name, country, region, minPopulation } = this.searchForm.value;

    const nameStr = (name || '').trim();
    const countryStr = (country || '').trim();
    const regionStr = (region || '').trim();

    if (!nameStr && !countryStr && !regionStr && !minPopulation) {
      return true;
    }

    const hasValidMinLength = nameStr.length >= 3 || countryStr.length >= 3 || regionStr.length >= 3 || minPopulation > 0;

    return !hasValidMinLength;
  }

  // Búsqueda limpia sin líneas duplicadas
  search() {
    if (this.isSearchInvalid()) return;

    this.loading = true;
    this.hasSearched = false;
    this.cities = [];

    const { name, country, region } = this.searchForm.value;

    this.destinationService.searchCities({ 
      partialName: name || 'a', 
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

  // Verificación flexible por ID y por Nombre de la ciudad
  isAlreadySaved(city: any): boolean {
    if (!city) return false;

    const cityId = city.id || city.wikiDataId;
    const cityName = (city.name || city.nombre || '').toLowerCase().trim();

    return this.favoriteCityIds.has(cityId) || this.favoriteCityIds.has(cityName);
  }

  // Guardar en favoritos y actualizar estado en tiempo real
  saveToFavorites(city: any) {
    if (this.isAlreadySaved(city)) return;

    this.loading = true;

    this.destinationService.importFromGeoDb(city.id).subscribe({
      next: (destinoGuardado: any) => {
        const destinoId = destinoGuardado.id || destinoGuardado;

        this.restService.request<any, void>({
          method: 'POST',
          url: `/api/app/favorites/agregar/${destinoId}`
        }).subscribe({
          next: () => {
            this.toaster.success(`¡${city.name} se guardó en tus favoritos!`);

            // Agrega al Set inmediatamente para deshabilitar el botón en pantalla
            const cityId = city.id || city.wikiDataId;
            const cityName = (city.name || city.nombre || '').toLowerCase().trim();
            if (cityId) this.favoriteCityIds.add(cityId);
            if (cityName) this.favoriteCityIds.add(cityName);

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