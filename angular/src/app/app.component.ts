import { Component, OnInit } from '@angular/core';
import { DynamicLayoutComponent, RoutesService } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
  `,
  imports: [LoaderBarComponent, DynamicLayoutComponent],
})
export class AppComponent implements OnInit {
  constructor(private routesService: RoutesService) {}

  ngOnInit() {
    // Lo agregamos directamente a la barra lateral principal
    this.routesService.add([
      {
        path: '/admin',
        name: 'Panel Admin',
        order: 5,
        iconClass: 'fa fa-chart-line',
      },
    ]);
  }
}