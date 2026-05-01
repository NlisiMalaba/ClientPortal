import {
  ChangeDetectionStrategy,
  Component,
  forwardRef,
  input,
} from '@angular/core';
import {
  ControlValueAccessor,
  NG_VALUE_ACCESSOR,
} from '@angular/forms';

import { cn } from '@/components/lib/utils';

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

@Component({
  selector: 'ui-select',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true,
    },
  ],
  template: `
    <select
      [value]="value"
      [disabled]="disabled"
      [class]="classes()"
      (change)="onSelect($event)"
      (blur)="onBlur()"
    >
      @if (placeholder() !== '') {
        <option value="" [disabled]="required()">{{ placeholder() }}</option>
      }
      @for (option of options(); track option.value) {
        <option [value]="option.value" [disabled]="option.disabled === true">
          {{ option.label }}
        </option>
      }
    </select>
  `,
})
export class SelectComponent implements ControlValueAccessor {
  readonly options = input<ReadonlyArray<SelectOption>>([]);
  readonly placeholder = input('Select an option');
  readonly required = input(false);
  readonly class = input('');

  value = '';
  disabled = false;

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  classes(): string {
    return cn(
      'h-9 w-full rounded-md border border-input bg-background px-3 py-2 text-sm outline-none transition-colors',
      'disabled:cursor-not-allowed disabled:opacity-50',
      'focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/40',
      this.class(),
    );
  }

  writeValue(value: string | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled = disabled;
  }

  onSelect(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.value = target.value;
    this.onChange(this.value);
  }

  onBlur(): void {
    this.onTouched();
  }
}
