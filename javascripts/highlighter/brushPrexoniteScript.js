SyntaxHighlighter.brushes.Prexonite = function()
{
	var keywords =	'add and as asm break build catch command continue coroutine declare disabled' +
	    'do does else enabled false finally for foreach function goto if in is macro mod new not null' +
	    'or ref return static throw true try to unless until using var while xor yield';
	    
	var commands  = 'call call\cc call\macro call\member call\tail caller char concat debug dispose loadassembly' + 
	    'pair print println unbind all count distinct each exists foldl foldr forall frequency groupby' +
	    'intersect limit list map skip sort where abs ceiling cos exp floor log max min pi round' +
	    'sin sqrt tan setcenter  setleft setright';
	    
	var types = 'Bool Char Hash Int List Null Object Real String Structure';
	
	this.regexList = [
		{ regex: SyntaxHighlighter.regexLib.singleLineCComments,	css: 'comments' },			// one line comments
		{ regex: SyntaxHighlighter.regexLib.multiLineCComments,		css: 'comments' },			// multiline comments
		{ regex: SyntaxHighlighter.regexLib.doubleQuotedString,		css: 'string' },			// double quoted strings
		
		{ regex: new RegExp(this.getKeywords(keywords), 'gm'),		css: 'keyword' },			// keywords
		{ regex: new RegExp(this.getKeywords(commands), 'gm'),		css: 'color1' },			// keywords
		{ regex: new RegExp(this.getKeywords(types), 'gm'),		css: 'color2' }			// keywords
		];
	
};

SyntaxHighlighter.brushes.Prexonite.prototype	= new SyntaxHighlighter.Highlighter();
SyntaxHighlighter.brushes.Prexonite.aliases	= ['pxs','prexonite', 'prx'];
