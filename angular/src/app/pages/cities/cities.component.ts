import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; 
import { ToasterService } from '@abp/ng.theme.shared';
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

saveToDestinations(city: any) {
  this.destinationService.importFromGeoDb(city.id) 
    .subscribe({
      next: () => {
        this.toaster.success(`¡${city.name} se guardó con éxito!`);
      },
      error: (err: any) => {
        console.error(err); 
        this.toaster.error('Error al guardar el destino.');
      }
    });
}
}