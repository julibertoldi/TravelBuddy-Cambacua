import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CalificacionDto extends AuditedEntityDto<string> {
  destinoId?: string;
  usuarioId?: string;
  estrellas: number;
  titulo?: string;
  comentario?: string;
  palabrasClave?: string;
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
  titulo: string;
  comentario: string;
  palabrasClave?: string;
}
