---
layout: page
title: Messing around with syntax and semantics&hellip;
---
&hellip;and see what happens. An experiment in language design an implementation.

```pxs
function main() {
  println("Hello World");
}
```

Another implementation of JavaScript? No, much much worse:

```pxs
function flipperize(xs) = 
  xs 
    >> where(? is not null) 
    >> map(x => if(x mod 2 == 0) "flip" 
                            else "flop");
```

That, and tons of meta-programming await you in this dynamically typed, 
somewhat object-oriented<sup><a href="#f1">1</a></sup>, interpreted abomination of a scripting language.

<div id="footnotes">
  <h3>Footnotes</h3>
  <ol>
    <li>      
      <a name="f1">&nbsp;</a>Yes, that's a thing. Prexonite Script allows you to <em>consume</em> objects, methods, properties, index accessors, 
  etc. but there is no way to define classes or even prototypes, like with JavaScript. Having said that, there are ways
  to define custom constructors that return _"objects"_, however, 
  looking for `.prototype` will be in vain.
    </li>
  </ol>
</div>