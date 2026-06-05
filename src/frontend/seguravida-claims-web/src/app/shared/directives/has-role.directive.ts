import { Directive, TemplateRef, ViewContainerRef, effect, inject, input } from '@angular/core';

import { UserRole } from '../../core/auth/auth.models';
import { AuthService } from '../../core/auth/auth.service';

@Directive({
  selector: '[appHasRole]',
  standalone: true,
})
export class HasRoleDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainerRef = inject(ViewContainerRef);
  private readonly auth = inject(AuthService);
  private hasView = false;

  readonly appHasRole = input.required<UserRole | UserRole[]>();

  constructor() {
    effect(() => {
      this.auth.role();
      this.updateView();
    });
  }

  private updateView(): void {
    const rolesInput = this.appHasRole();
    const roles = Array.isArray(rolesInput) ? rolesInput : [rolesInput];
    const shouldShow = this.auth.hasAnyRole(roles);

    if (shouldShow && !this.hasView) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
      this.hasView = true;
      return;
    }

    if (!shouldShow && this.hasView) {
      this.viewContainerRef.clear();
      this.hasView = false;
    }
  }
}
