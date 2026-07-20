import { mapEnumToOptions } from '@abp/ng.core';

export enum ExperienciaValoracion {
  Mala = 1,
  Regular = 2,
  Buena = 3,
  MuyBuena = 4,
  Excelente = 5,
}

export const experienciaValoracionOptions = mapEnumToOptions(ExperienciaValoracion);
