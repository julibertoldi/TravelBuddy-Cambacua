import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { ListService, PagedResultDto, ConfigStateService } from '@abp/ng.core';
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
  providers: [ListService, ExperienciaService],
})
export class ExperiencesComponent implements OnInit {
  experiences: PagedResultDto<ExperienciaDto> = {
    items: [],
    totalCount: 0,
  };

  filterForm!: FormGroup;
  form!: FormGroup;
  isModalOpen = false;
  selectedExperienceId?: string;

  readonly valoraciones = [
    { value: ExperienciaValoracion.Mala, label: 'Mala' },
    { value: ExperienciaValoracion.Regular, label: 'Regular' },
    { value: ExperienciaValoracion.Buena, label: 'Buena' },
    { value: ExperienciaValoracion.MuyBuena, label: 'Muy buena' },
    { value: ExperienciaValoracion.Excelente, label: 'Excelente' },
  ];

  constructor(
    public readonly list: ListService,
    private readonly experienciaService: ExperienciaService,
    private readonly formBuilder: FormBuilder,
    private readonly confirmationService: ConfirmationService,
    private readonly configState: ConfigStateService,
  ) {}

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

  private buildFilterForm(): void {
    this.filterForm = this.formBuilder.group({
      destinoId: [''],
      valoracion: [null],
      keyword: [''],
    });
  }

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

  search(): void {
    this.list.get();
  }

  clearFilters(): void {
    this.filterForm.reset({
      destinoId: '',
      valoracion: null,
      keyword: '',
    });

    this.list.get();
  }

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

  editExperience(id: string): void {
    this.experienciaService.get(id).subscribe((experience: ExperienciaDto) => {
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

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const input: CreateUpdateExperienciaDto = {
      destinoId: this.form.value.destinoId,
      usuarioId: this.configState.getOne('currentUser')?.id || '',
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

  closeModal(): void {
    this.isModalOpen = false;
    this.selectedExperienceId = undefined;
    this.form.reset();
  }

  getValoracionLabel(value?: number): string {
    const option = this.valoraciones.find(
      item => item.value === value,
    );

    return option?.label ?? 'Sin valoración';
  }
}