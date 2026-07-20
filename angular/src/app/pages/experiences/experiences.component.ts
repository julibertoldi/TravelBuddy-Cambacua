// Importa herramientas de Angular, formularios reactivos
// y servicios de ABP necesarios para listar y administrar experiencias.
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { ListService, PagedResultDto } from '@abp/ng.core';
import {
  Confirmation,
  ConfirmationService,
  ThemeSharedModule,
} from '@abp/ng.theme.shared';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import {
  CreateUpdateExperienciaDto,
  ExperienciaDto,
  ExperienciaGetListInput,
  ExperienciaService,
    ExperienciaValoracion,
} from '../../proxy/experiencias';
// Define el componente de la pantalla de experiencias.
// Usa su archivo HTML, sus estilos y los módulos necesarios.
@Component({
  selector: 'app-experiences',
  templateUrl: './experiences.component.html',
  styleUrls: ['./experiences.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ThemeSharedModule,
    NgbModule,
  ],
  providers: [ListService],
})
export class ExperiencesComponent implements OnInit {
    // Guarda la lista paginada de experiencias que devuelve el backend.
  experiences: PagedResultDto<ExperienciaDto> = {
    items: [],
    totalCount: 0,
  };
// Formulario para filtrar experiencias y formulario para crear o editar.
  filterForm!: FormGroup;
  form!: FormGroup;
// Controla la apertura del formulario y guarda el ID
// de la experiencia seleccionada cuando se está editando.
  isModalOpen = false;
  selectedExperienceId?: string;
// Opciones de valoración que se mostrarán en los selectores.
readonly valoraciones = [
  { value: ExperienciaValoracion.Mala, label: 'Mala' },
  { value: ExperienciaValoracion.Regular, label: 'Regular' },
  { value: ExperienciaValoracion.Buena, label: 'Buena' },
  { value: ExperienciaValoracion.MuyBuena, label: 'Muy buena' },
  { value: ExperienciaValoracion.Excelente, label: 'Excelente' },
];
// Inyecta los servicios necesarios para listar,
// comunicarse con el backend, crear formularios y confirmar eliminaciones.
  constructor(
    public readonly list: ListService,
    private readonly experienciaService: ExperienciaService,
    private readonly formBuilder: FormBuilder,
    private readonly confirmationService: ConfirmationService,
  ) {}
// Inicializa los formularios y conecta el listado
// con el servicio del backend aplicando los filtros seleccionados.
  ngOnInit(): void {
    this.buildFilterForm();
    this.buildForm();

    const experienceStreamCreator = (
      query: ExperienciaGetListInput,
    ) => {
      const filters = this.filterForm.value;

      return this.experienciaService.getList({
        ...query,
        destinoId: filters.destinoId || undefined,
        valoracion: filters.valoracion || undefined,
        keyword: filters.keyword || undefined,
      });
    };

    this.list
      .hookToQuery(experienceStreamCreator)
      .subscribe(response => {
        this.experiences = response;
      });
  }
// Crea el formulario usado para filtrar por destino,
// valoración y palabra clave.
  private buildFilterForm(): void {
    this.filterForm = this.formBuilder.group({
      destinoId: [''],
      valoracion: [null],
      keyword: [''],
    });
  }
// Crea el formulario para agregar o modificar una experiencia
// y aplica las mismas validaciones definidas en el backend.
  private buildForm(): void {
    this.form = this.formBuilder.group({
      destinoId: ['', Validators.required],
      titulo: [
        '',
        [Validators.required, Validators.maxLength(128)],
      ],
      descripcion: [
        '',
        [Validators.required, Validators.maxLength(1024)],
      ],
      valoracion: [null, Validators.required],
      palabrasClave: [''],
    });
  }
// Ejecuta nuevamente la consulta usando los filtros actuales.
  search(): void {
    this.list.get();
  }
// Limpia todos los filtros y vuelve a cargar el listado completo.
  clearFilters(): void {
    this.filterForm.reset({
      destinoId: '',
      valoracion: null,
      keyword: '',
    });

    this.list.get();
  }
// Abre el formulario vacío para crear una nueva experiencia.
  createExperience(): void {
    this.selectedExperienceId = undefined;

    this.form.reset({
      destinoId: '',
      titulo: '',
      descripcion: '',
      valoracion: null,
      palabrasClave: '',
    });

    this.isModalOpen = true;
  }
// Busca una experiencia por su ID,
// carga sus datos en el formulario y la prepara para editar.
  editExperience(id: string): void {
    this.experienciaService.get(id).subscribe(experience => {
      this.selectedExperienceId = id;

      this.form.patchValue({
        destinoId: experience.destinoId,
        titulo: experience.titulo,
        descripcion: experience.descripcion,
        valoracion: experience.valoracion,
        palabrasClave: experience.palabrasClave,
      });

      this.isModalOpen = true;
    });
  }
// Valida el formulario y crea o actualiza la experiencia
// según exista o no un ID seleccionado.
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const input: CreateUpdateExperienciaDto = {
      destinoId: this.form.value.destinoId,
      titulo: this.form.value.titulo,
      descripcion: this.form.value.descripcion,
      valoracion: this.form.value.valoracion,
      palabrasClave: this.form.value.palabrasClave || undefined,
    };

    const request = this.selectedExperienceId
      ? this.experienciaService.update(
          this.selectedExperienceId,
          input,
        )
      : this.experienciaService.create(input);

    request.subscribe(() => {
      this.closeModal();
      this.list.get();
    });
  }
// Pide confirmación y elimina la experiencia mediante el backend.
  deleteExperience(id: string): void {
    this.confirmationService
      .warn(
        '¿Seguro que querés eliminar esta experiencia?',
        'Confirmar eliminación',
      )
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }

        this.experienciaService
          .delete(id)
          .subscribe(() => this.list.get());
      });
  }
// Cierra el formulario y limpia los datos seleccionados.
  closeModal(): void {
    this.isModalOpen = false;
    this.selectedExperienceId = undefined;
    this.form.reset();
  }
// Convierte el valor numérico de la valoración
// en un texto entendible para mostrar en pantalla.
  getValoracionLabel(value?: number): string {
    const option = this.valoraciones.find(
      item => item.value === value,
    );

    return option?.label ?? 'Sin valoración';
  }
}