import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ExperienciaValoracion } from './experiencia-valoracion.enum';

export interface ExperienciaGetListInput extends PagedAndSortedResultRequestDto {
  destinoId?: string;
  valoracion?: ExperienciaValoracion;
  keyword?: string;
}

export interface CreateUpdateExperienciaDto {
  destinoId: string;
  usuarioId?: string; // 👈 Marcado como opcional para evitar errores al hacer la petición
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