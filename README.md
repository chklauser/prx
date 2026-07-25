# Prexonite Script

    function main() {
      println("Hello World");
    }

Another implementation of JavaScript? No, much much worse:


    function flipperize(xs) = 
      xs 
        >> where(? is not null) 
        >> map(x => if(x mod 2 == 0) "flip" 
                                else "flop");

That, and tons of meta-programming<sup><a href="#f1">1</a></sup> await you in this dynamically typed<sup><a href="#f2">2</a></sup>, 
somewhat object-aware<sup><a href="#f3">3</a></sup>, interpreted<sup><a href="#f4">4</a></sup> abomination of a scripting language.

<div id="footnotes">
  <h3>Footnotes</h3>
  <ol>
    <li>      
      <a name="f1">&nbsp;</a> In it's most unsafe and raw form: Imagine macros, which to the untrained eye look exactly like functions, that produce arbitrary AST nodes to be spliced into their callsites. Or format your hard drive. And then crash the compiler.
    </li>
    <li>      
      <a name="f2">&nbsp;</a>I guess you could make the argument that the type system in Prexonite Script is actually highly advanced by being able to deal with dependent types. In case you're into that kind of thing. Then again, that is usually not very impressive in dynamically typed settings. 
    </li>
    <li>      
      <a name="f3">&nbsp;</a>Yes, that's a thing. Prexonite Script allows you to <em>consume</em> objects, methods, properties, index accessors, 
  etc. but there is no way to define classes or even prototypes, like with JavaScript. Having said that, there are ways
  to define custom constructors that return <em>"objects"</em>, however, 
  looking for <code>.prototype</code> will be in vain.
    </li>
    <li>      
      <a name="f4">&nbsp;</a>Technically speaking, Prexonite Script is actually being compiled. Three times. A significant portion of Prexonite code runs as native machine code. And, after a mere couple of decades, CLR member calls even remember their overload resolution. We still check the answer&mdash;this is a dynamically typed abomination&mdash;but the pesky methods no longer need to prove that they have not swapped places since the last loop iteration.
    </li>
  </ol>
</div>
