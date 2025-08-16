import { Component } from '@angular/core';

import { ThemeItem } from "../theme-item/theme-item";
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../../core/services/theme.service';
import { Theme } from '../../../models/theme.model';

@Component({
  selector: 'app-theme-board',
  imports: [CommonModule, ThemeItem],
  templateUrl: './theme-board.html',
  styleUrl: './theme-board.css'
})
export class ThemeBoard {
  themes$: Observable<Theme[]>;

  constructor(private themeService: ThemeService) {
    this.themes$ = this.themeService.getThemes();
  }
}