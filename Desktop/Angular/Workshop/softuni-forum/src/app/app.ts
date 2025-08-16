import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './shared/components/header/header';
import { Footer } from './shared/components/footer/footer';

import { ThemeBoard } from './features/themes/theme-board/theme-board';
import { PostBoard } from './features/post-board/post-board';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, Footer, ThemeBoard, PostBoard],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
