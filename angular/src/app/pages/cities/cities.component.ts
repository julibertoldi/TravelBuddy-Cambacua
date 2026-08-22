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
  loading = false;

  constructor(
    private fb: FormBuilder,
    private destinationService: DestinationService,
    private restService: RestService, 
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    this.buildForm();
  }

  buildForm() {
    this.searchForm = this.fb.group({
      name: [''],          
      country: [''],        
      region: [''],          
      minPopulation: [null]  
    });
  }

  search() {
    this.loading = true;
    const { name, country, region } = this.searchForm.value;
    
    this.destinationService.searchCities({ 
      partialName: name, 
      pais: country || undefined, 
      region: region || undefined
    }).subscribe({
      next: (response: any) => {
        this.cities = response.cities || response.items || response || [];
        this.loading = false;
      },
      error: () => {
        this.toaster.error('Error al consultar la API externa.');
        this.loading = false;
      }
    });
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