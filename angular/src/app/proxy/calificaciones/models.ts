import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CalificacionDto extends AuditedEntityDto<string> {
  destinoId?: string;
  usuarioId?: string;
  estrellas: number;
  comentario?: string;
}

export interface CalificacionGetListInput extends PagedAndSortedResultRequestDto {
  destinoId?: string;
}

export interface CalificacionPromedioDto {
  promedio: number;
  totalCalificaciones: number;
}

export interface CreateUpdateCalificacionDto {
  destinoId: string;
  estrellas: number;
  comentario: string;
}
