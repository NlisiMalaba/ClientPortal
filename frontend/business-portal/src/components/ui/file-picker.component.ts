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

@Component({
  selector: 'ui-file-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FilePickerComponent),
      multi: true,
    },
  ],
  template: `
    <div [class]="wrapperClasses()">
      <input
        type="file"
        [attr.accept]="accept()"
        [disabled]="disabled"
        [class]="inputClasses()"
        (change)="onFileChange($event)"
        (blur)="onBlur()"
      />
      @if (selectedFileName !== '') {
        <p class="text-xs text-muted-foreground">{{ selectedFileName }}</p>
      }
    </div>
  `,
})
export class FilePickerComponent implements ControlValueAccessor {
  readonly accept = input('');
  readonly class = input('');

  file: File | null = null;
  selectedFileName = '';
  disabled = false;

  private onChange: (value: File | null) => void = () => {};
  private onTouched: () => void = () => {};

  wrapperClasses(): string {
    return cn('flex flex-col gap-2', this.class());
  }

  inputClasses(): string {
    return cn(
      'block w-full rounded-md border border-input bg-background px-3 py-2 text-sm',
      'file:mr-3 file:rounded-md file:border-0 file:bg-muted file:px-3 file:py-1.5 file:text-sm',
      'disabled:cursor-not-allowed disabled:opacity-50',
    );
  }

  writeValue(value: File | null): void {
    this.file = value;
    this.selectedFileName = value?.name ?? '';
  }

  registerOnChange(fn: (value: File | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled = disabled;
  }

  onFileChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.item(0) ?? null;

    this.file = file;
    this.selectedFileName = file?.name ?? '';
    this.onChange(file);
  }

  onBlur(): void {
    this.onTouched();
  }
}
