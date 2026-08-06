import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateDestinationDto {
  nombre: string;
  descripcion?: string;
  ubicacion?: string;
  precio: number;
  imagenUrl?: string;
  disponible: boolean;
  categoriaId?: string;
  population: number;
  latitude: number;
  longitude: number;
}

export interface DestinationDto extends AuditedEntityDto<string> {
  id?: string;
  nombre?: string;
  descripcion?: string;
  ubicacion?: string;
  precio: number;
  imagenUrl?: string;
  disponible: boolean;
  fechaCreacion?: string;
  fechaActualizacion?: string;
  categoriaId?: string;
  categoriaName?: string;
  geoDbCityId?: number;
  population: number;
  latitude: number;
  longitude: number;
  lastUpdated?: string;
}
