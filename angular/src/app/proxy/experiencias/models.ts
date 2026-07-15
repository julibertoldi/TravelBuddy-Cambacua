import type { ExperienciaValoracion } from './experiencia-valoracion.enum';
import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateExperienciaDto {
  destinoId: string;
  titulo: string;
  descripcion: string;
  valoracion: ExperienciaValoracion;
  palabrasClave?: string;
}

export interface ExperienciaDto extends AuditedEntityDto<string> {
  destinoId?: string;
  usuarioId?: string;
  titulo?: string;
  descripcion?: string;
  valoracion?: ExperienciaValoracion;
  palabrasClave?: string;
}

export interface ExperienciaGetListInput extends PagedAndSortedResultRequestDto {
  destinoId?: string;
  valoracion?: ExperienciaValoracion;
  keyword?: string;
}
