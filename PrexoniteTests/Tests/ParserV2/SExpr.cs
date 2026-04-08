// Prexonite – ParserV2 Tests – S-expression serializer for AST nodes.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Prexonite.Compiler.ParserV2;
using Prexonite.Compiler.ParserV2.Ast;
using Prexonite.Compiler.ParserV2.Lexing;

namespace PrexoniteTests.Tests.ParserV2;

/// <summary>
/// Produces indented S-expressions from AST nodes for test assertions.
/// </summary>
public static class SExpr
{
    public static string Serialize(Node node)
    {
        var sb = new StringBuilder();
        Write(node, sb, 0);
        return sb.ToString();
    }

    static void Write(Node? node, StringBuilder sb, int indent)
    {
        if (node == null) { sb.Append("null"); return; }

        switch (node)
        {
            // ── Compilation unit ─────────────────────────────────────────────
            case CompilationUnit cu:
                WriteList(sb, indent, "cu", cu.Declarations.Cast<Node>().ToArray());
                break;

            // ── Declarations ─────────────────────────────────────────────────
            case FunctionDecl fn:
                WriteFunctionDecl(fn, sb, indent);
                break;
            case GlobalVarDecl gv:
                WriteGlobalVarDecl(gv, sb, indent);
                break;
            case NamespaceDecl ns:
                WriteNamespaceDecl(ns, sb, indent);
                break;
            case NamespaceImportDecl nsi:
                WriteList(sb, indent, "ns-import", nsi.Specs.Cast<Node>().ToArray());
                break;
            case DeclareListDecl dl:
                WriteDeclareLiftDecl(dl, sb, indent);
                break;
            case DeclareBlockDecl db:
                WriteDeclareBlockDecl(db, sb, indent);
                break;
            case DeclareMExprDecl dm:
                WriteList(sb, indent, "declare-mexpr",
                    dm.Bindings.Select(b => (Node)b).ToArray());
                break;
            case BuildBlockDecl bb:
                WriteList(sb, indent, "build", new Node[] { bb.Body });
                break;
            case GlobalCodeDecl gc:
                WriteList(sb, indent, "global-code", new Node[] { gc.Body });
                break;
            case ModuleMetaDecl mm:
                WriteList(sb, indent, "meta", new Node[] { mm.Entry });
                break;
            case ErrorDecl ed:
                sb.Append($"(error-decl {QuoteString(ed.Message)})");
                break;

            // ── Statements ───────────────────────────────────────────────────
            case ExprStmt es:
                Write(es.Expression, sb, indent);
                break;
            case ReturnStmt rs:
                if (rs.Expression == null) sb.Append("(return)");
                else WriteList(sb, indent, "return", new Node[] { rs.Expression });
                break;
            case YieldStmt ys:
                if (ys.Expression == null) sb.Append("(yield)");
                else WriteList(sb, indent, "yield", new Node[] { ys.Expression });
                break;
            case BreakStmt:
                sb.Append("(break)");
                break;
            case ContinueStmt:
                sb.Append("(continue)");
                break;
            case GotoStmt gs:
                sb.Append($"(goto {QuoteString(gs.Label)})");
                break;
            case LabelStmt ls:
                sb.Append($"(label {QuoteString(ls.Name)})");
                break;
            case ThrowStmt ts:
                WriteList(sb, indent, "throw", new Node[] { ts.Expression });
                break;
            case LetBindingStmt lb:
                WriteLetBindingStmt(lb, sb, indent);
                break;
            case IfStmt ifs:
                WriteIfStmt(ifs, sb, indent);
                break;
            case WhileStmt ws:
                WriteWhileStmt(ws, sb, indent);
                break;
            case ForStmt frs:
                WriteForStmt(frs, sb, indent);
                break;
            case ForeachStmt fes:
                WriteForeachStmt(fes, sb, indent);
                break;
            case TryCatchFinallyStmt tcf:
                WriteTryCatch(tcf, sb, indent);
                break;
            case UsingStmt us:
                WriteList(sb, indent, "using", new Node[] { us.Resource, us.Body });
                break;
            case AsmStmt asmS:
                WriteAsmBlock(sb, indent, "asm", asmS.Instructions);
                break;
            case NestedFunctionStmt nfs:
                Write(nfs.Function, sb, indent);
                break;
            case ErrorStmt errS:
                sb.Append($"(error-stmt {QuoteString(errS.Message)})");
                break;

            // ── Expressions ──────────────────────────────────────────────────
            case IntLit il:
                sb.Append(il.Value.ToString());
                break;
            case RealLit rl:
                sb.Append(rl.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;
            case BoolLit bl:
                sb.Append(bl.Value ? "true" : "false");
                break;
            case NullLit:
                sb.Append("null");
                break;
            case StringLit sl:
                sb.Append(QuoteString(sl.Value));
                break;
            case InterpolatedString istr:
                WriteInterpolatedString(istr, sb, indent);
                break;
            case NameExpr ne:
                sb.Append($"(id {QuoteString(ne.Name)})");
                break;
            case RefExpr re:
                sb.Append($"(ref {re.PointerCount} {QuoteString(re.Name)})");
                break;
            case PlaceholderExpr pe:
                sb.Append(pe.Index.HasValue ? $"(placeholder {pe.Index})" : "(placeholder)");
                break;
            case LocalVarDecl lvd:
                WriteLocalVarDecl(lvd, sb, indent);
                break;
            case ListLit ll:
                WriteList(sb, indent, "list", ll.Elements.Cast<Node>().ToArray());
                break;
            case HashLit hl:
                WriteList(sb, indent, "hash", hl.Elements.Cast<Node>().ToArray());
                break;
            case KeyValueExpr kv:
                WriteList(sb, indent, "kvp", new Node[] { kv.Key, kv.Value });
                break;
            case BinaryExpr bin:
                WriteList(sb, indent, BinOpName(bin.Op), new Node[] { bin.Left, bin.Right });
                break;
            case UnaryExpr un:
                WriteList(sb, indent, UnaryOpName(un.Op), new Node[] { un.Operand });
                break;
            case CoalesceExpr ce:
                WriteList(sb, indent, "??", ce.Operands.Cast<Node>().ToArray());
                break;
            case AssignExpr ae:
                WriteAssignExpr(ae, sb, indent);
                break;
            case CastAssignExpr cae:
                WriteList(sb, indent, "~=", new Node[] { cae.Target, cae.Type });
                break;
            case CallExpr call:
                WriteCallExpr(call, sb, indent);
                break;
            case MemberAccessExpr ma:
                WriteList(sb, indent, ".", new Node[] { ma.Subject, new _StrNode(ma.Member) });
                break;
            case MemberCallExpr mc:
                WriteMemberCallExpr(mc, sb, indent);
                break;
            case IndirectCallExpr ic:
                WriteIndirectCallExpr(ic, sb, indent);
                break;
            case IndexExpr idx:
                WriteList(sb, indent, "[]", new Node[] { idx.Subject }.Concat(idx.Indices.Cast<Node>()));
                break;
            case AppendRightExpr arr:
                WriteList(sb, indent, ">>", new Node[] { arr.Left, arr.Right });
                break;
            case AppendLeftExpr aleft:
                WriteList(sb, indent, "<<", new Node[] { aleft.Callee }.Concat(aleft.PrependArgs.Cast<Node>()));
                break;
            case TypeCastExpr tc:
                WriteList(sb, indent, "~", new Node[] { tc.Subject, tc.Type });
                break;
            case TypeCheckExpr tck:
                WriteList(sb, indent, tck.IsNegated ? "is-not" : "is", new Node[] { tck.Subject, tck.Type });
                break;
            case ConditionalExpr cond:
                WriteList(sb, indent, cond.IsNegated ? "unless-expr" : "if-expr",
                    new Node[] { cond.Condition, cond.Then, cond.Else });
                break;
            case ThrowExpr te:
                WriteList(sb, indent, "throw-expr", new Node[] { te.Value });
                break;
            case NewExpr newE:
                WriteNewExpr(newE, sb, indent);
                break;
            case LambdaExpr lam:
                WriteLambdaExpr(lam, sb, indent);
                break;
            case LazyExpr lazy:
                WriteList(sb, indent, "lazy", new Node[] { lazy.Body });
                break;
            case CoroutineExpr cor:
                WriteCoroutineExpr(cor, sb, indent);
                break;
            case AsmExpr asmE:
                WriteAsmBlock(sb, indent, "asm-expr", asmE.Instructions);
                break;
            case StaticCallExpr sc:
                WriteStaticCallExpr(sc, sb, indent);
                break;
            case ErrorNode en:
                sb.Append($"(error {QuoteString(en.Message)})");
                break;

            // ── Types ─────────────────────────────────────────────────────────
            case PrxTypeExpr pt:
                WritePrxTypeExpr(pt, sb);
                break;
            case ClrTypeExpr ct:
                sb.Append($"(clr-type {QuoteString(ct.FullName)})");
                break;
            case ErrorTypeExpr ete:
                sb.Append($"(error-type {QuoteString(ete.Message)})");
                break;

            // ── Block ─────────────────────────────────────────────────────────
            case Block blk:
                WriteBlock(blk, sb, indent);
                break;

            // ── Lambda body ──────────────────────────────────────────────────
            case LambdaBlockBody lbb:
                WriteBlock(lbb.Statements, sb, indent);
                break;
            case LambdaExprBody leb:
                Write(leb.Expression, sb, indent);
                break;

            // ── Function body ────────────────────────────────────────────────
            case FunctionBlockBody fbb:
                WriteBlock(fbb.Statements, sb, indent);
                break;
            case FunctionExprBody feb:
                Write(feb.Expression, sb, indent);
                break;

            // ── Meta entries ─────────────────────────────────────────────────
            case MetaBoolEntry mbe:
                sb.Append($"(is {(mbe.Value ? "" : "not ")}{QuoteString(mbe.Key)})");
                break;
            case MetaSwitchEntry mse:
                sb.Append($"(switch {QuoteString(mse.Key)} {(mse.Value ? "true" : "false")})");
                break;
            case MetaValueEntry mve:
                WriteList(sb, indent, "meta-val", new Node[] { new _StrNode(mve.Key), mve.Value });
                break;
            case MetaAddEntry mae:
                WriteList(sb, indent, "meta-add", new Node[] { new _StrNode(mae.Key), mae.Addition });
                break;

            // ── MExpr ─────────────────────────────────────────────────────────
            case MExprAtom mea:
                WriteMExprAtom(mea, sb);
                break;
            case MExprList mel:
                WriteList(sb, indent, "mexpr-list",
                    new[] { new _StrNode(mel.Head) }.Concat(mel.Args.Cast<Node>()).ToArray());
                break;

            // ── Asm instructions ──────────────────────────────────────────────
            case AsmVarDecl avd:
                sb.Append($"(asm-var {(avd.IsRef ? "ref " : "")}{string.Join(" ", avd.Names.Select(QuoteString))})");
                break;
            case AsmLabelDecl ald:
                sb.Append($"(asm-label {QuoteString(ald.Name)})");
                break;
            case AsmOpInstr aoi:
                WriteAsmOpInstr(aoi, sb);
                break;

            // ── Namespace transfer ────────────────────────────────────────────
            case NsTransferSpec nts:
                WriteNsTransferSpec(nts, sb, indent);
                break;
            case NsWildcardDirective:
                sb.Append("(ns-*)");
                break;
            case NsRenameDirective nrd:
                if (nrd.ExternalName == nrd.InternalName)
                    sb.Append($"(ns-rename {QuoteString(nrd.ExternalName)})");
                else
                    sb.Append($"(ns-rename {QuoteString(nrd.ExternalName)} => {QuoteString(nrd.InternalName)})");
                break;
            case NsDropDirective ndd:
                sb.Append($"(ns-drop {QuoteString(ndd.Name)})");
                break;
            case NsImportClause nic:
                WriteList(sb, indent, "ns-import-clause", nic.Specs.Cast<Node>().ToArray());
                break;

            // ── FormalParam, QualifiedName ────────────────────────────────────
            case FormalParam fp:
                sb.Append(fp.IsRef ? $"(param ref {QuoteString(fp.Name)})" : $"(param {QuoteString(fp.Name)})");
                break;
            case QualifiedName qn:
                sb.Append($"(qname {string.Join("." , qn.Parts.Select(QuoteString))})");
                break;

            // ── Internal helper ───────────────────────────────────────────────
            case _StrNode sn:
                sb.Append(QuoteString(sn.Value));
                break;

            // ── MExprBinding, DeclareItem ─────────────────────────────────────
            case MExprBinding mxb:
                WriteList(sb, indent, "binding", new Node[] { new _StrNode(mxb.Alias), mxb.Expr });
                break;
            case DeclareItem di:
                WriteDeclareItem(di, sb);
                break;
            case LetBinding lbind:
                WriteLetBinding(lbind, sb, indent);
                break;
            case CatchClause cc:
                WriteList(sb, indent, "catch", new Node[] { cc.ExceptionVar, cc.Body });
                break;

            default:
                sb.Append($"(unknown:{node.GetType().Name})");
                break;
        }
    }

    // ── Writers ──────────────────────────────────────────────────────────────

    static void WriteBlock(Block block, StringBuilder sb, int indent)
    {
        if (block.Statements.IsEmpty)
        {
            sb.Append("(block)");
            return;
        }
        sb.Append("(block");
        foreach (var stmt in block.Statements)
        {
            sb.Append('\n');
            sb.Append(Pad(indent + 2));
            Write(stmt, sb, indent + 2);
        }
        sb.Append(')');
    }

    static void WriteFunctionDecl(FunctionDecl fn, StringBuilder sb, int indent)
    {
        sb.Append('(');
        sb.Append(fn.Kind switch
        {
            FunctionKind.Lazy => "lazy-fn",
            FunctionKind.Coroutine => "coroutine-fn",
            FunctionKind.Macro => "macro-fn",
            _ => "fn"
        });
        sb.Append(' ');
        sb.Append(fn.PrimaryName != null ? QuoteString(fn.PrimaryName) : "null");

        // Aliases
        if (fn.Aliases.Length > 0)
        {
            sb.Append(" (aliases");
            foreach (var a in fn.Aliases) { sb.Append(' '); sb.Append(QuoteString(a)); }
            sb.Append(')');
        }

        // Meta
        if (fn.Meta.Length > 0)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            sb.Append("(meta");
            foreach (var m in fn.Meta)
            {
                sb.Append(' ');
                Write(m, sb, indent + 4);
            }
            sb.Append(')');
        }

        // Params
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        sb.Append("(params");
        foreach (var p in fn.Parameters)
        {
            sb.Append(' ');
            Write(p, sb, indent + 4);
        }
        sb.Append(')');

        // Import clause
        if (fn.ImportClause != null)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            Write(fn.ImportClause, sb, indent + 2);
        }

        // Body
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        sb.Append("(body");
        sb.Append('\n'); sb.Append(Pad(indent + 4));
        Write(fn.Body, sb, indent + 4);
        sb.Append(')');

        sb.Append(')');
    }

    static void WriteGlobalVarDecl(GlobalVarDecl gv, StringBuilder sb, int indent)
    {
        sb.Append('(');
        sb.Append(gv.IsRef ? "global-ref" : "global-var");
        sb.Append(' ');
        sb.Append(gv.PrimaryName != null ? QuoteString(gv.PrimaryName) : "null");
        if (gv.Aliases.Length > 0)
        {
            sb.Append(" (aliases");
            foreach (var a in gv.Aliases) { sb.Append(' '); sb.Append(QuoteString(a)); }
            sb.Append(')');
        }
        if (gv.Meta.Length > 0)
        {
            foreach (var m in gv.Meta) { sb.Append(' '); Write(m, sb, indent + 2); }
        }
        if (gv.Initializer != null)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            sb.Append("(init ");
            Write(gv.Initializer, sb, indent + 4);
            sb.Append(')');
        }
        sb.Append(')');
    }

    static void WriteNamespaceDecl(NamespaceDecl ns, StringBuilder sb, int indent)
    {
        sb.Append("(namespace");
        sb.Append(' ');
        Write(ns.Name, sb, indent + 2);

        if (ns.ImportSpecs.Length > 0)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            sb.Append("(import");
            foreach (var s in ns.ImportSpecs) { sb.Append(' '); Write(s, sb, indent + 4); }
            sb.Append(')');
        }

        if (ns.Body.Length > 0)
        {
            foreach (var d in ns.Body)
            {
                sb.Append('\n'); sb.Append(Pad(indent + 2));
                Write(d, sb, indent + 2);
            }
        }

        if (ns.Export != null)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            WriteExportSpec(ns.Export, sb, indent + 2);
        }

        sb.Append(')');
    }

    static void WriteExportSpec(NsExportSpec spec, StringBuilder sb, int indent)
    {
        switch (spec)
        {
            case NsExportAll:
                sb.Append("(export *)");
                break;
            case NsExportDirectives ned:
                sb.Append("(export");
                foreach (var d in ned.Directives) { sb.Append(' '); Write(d, sb, indent); }
                sb.Append(')');
                break;
            case NsExportSpecs nes:
                sb.Append("(export-specs");
                foreach (var s in nes.Specs) { sb.Append(' '); Write(s, sb, indent); }
                sb.Append(')');
                break;
        }
    }

    static void WriteDeclareLiftDecl(DeclareListDecl dl, StringBuilder sb, int indent)
    {
        sb.Append("(declare");
        if (dl.IsRef) sb.Append(" ref");
        if (dl.EntityKind != null) { sb.Append(' '); sb.Append(dl.EntityKind); }
        foreach (var item in dl.Items)
        {
            sb.Append(' ');
            Write(item, sb, indent + 2);
        }
        sb.Append(')');
    }

    static void WriteDeclareBlockDecl(DeclareBlockDecl db, StringBuilder sb, int indent)
    {
        sb.Append("(declare-block");
        if (db.UsingModule != null) { sb.Append($" (using {QuoteString(db.UsingModule)})"); }
        foreach (var e in db.Entries)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            Write(e, sb, indent + 2);
        }
        sb.Append(')');
    }

    static void WriteDeclareItem(DeclareItem di, StringBuilder sb)
    {
        sb.Append($"(item {QuoteString(di.Name)}");
        if (di.ModuleName != null) sb.Append($" /{QuoteString(di.ModuleName)}");
        if (di.Alias != null) sb.Append($" as {QuoteString(di.Alias)}");
        sb.Append(')');
    }

    static void WriteLetBindingStmt(LetBindingStmt lb, StringBuilder sb, int indent)
    {
        sb.Append("(let");
        foreach (var b in lb.Bindings)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            Write(b, sb, indent + 2);
        }
        sb.Append(')');
    }

    static void WriteLetBinding(LetBinding b, StringBuilder sb, int indent)
    {
        sb.Append($"(bind {QuoteString(b.Name)}");
        if (b.Initializer != null)
        {
            sb.Append(' ');
            Write(b.Initializer, sb, indent + 2);
        }
        sb.Append(')');
    }

    static void WriteIfStmt(IfStmt ifs, StringBuilder sb, int indent)
    {
        sb.Append('(');
        sb.Append(ifs.IsNegated ? "unless" : "if");
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(ifs.Condition, sb, indent + 2);
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(ifs.Then, sb, indent + 2);
        if (ifs.Else != null)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            sb.Append("(else\n"); sb.Append(Pad(indent + 4));
            Write(ifs.Else, sb, indent + 4);
            sb.Append(')');
        }
        sb.Append(')');
    }

    static void WriteWhileStmt(WhileStmt ws, StringBuilder sb, int indent)
    {
        string kind = ws.IsPostCondition
            ? (ws.IsNegated ? "do-until" : "do-while")
            : (ws.IsNegated ? "until" : "while");
        sb.Append($"({kind}");
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(ws.Condition, sb, indent + 2);
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(ws.Body, sb, indent + 2);
        sb.Append(')');
    }

    static void WriteForStmt(ForStmt fs, StringBuilder sb, int indent)
    {
        sb.Append("(for");
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        WriteBlock(fs.Init, sb, indent + 2);
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        if (fs.Condition != null) Write(fs.Condition, sb, indent + 2);
        else sb.Append("null");
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        WriteBlock(fs.Next, sb, indent + 2);
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(fs.Body, sb, indent + 2);
        sb.Append(')');
    }

    static void WriteForeachStmt(ForeachStmt fes, StringBuilder sb, int indent)
    {
        sb.Append("(foreach");
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(fes.Element, sb, indent + 2);
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(fes.List, sb, indent + 2);
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(fes.Body, sb, indent + 2);
        sb.Append(')');
    }

    static void WriteTryCatch(TryCatchFinallyStmt tcf, StringBuilder sb, int indent)
    {
        sb.Append("(try");
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(tcf.Try, sb, indent + 2);
        if (tcf.Catch != null)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            Write(tcf.Catch, sb, indent + 2);
        }
        if (tcf.Finally != null)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            sb.Append("(finally\n"); sb.Append(Pad(indent + 4));
            Write(tcf.Finally, sb, indent + 4);
            sb.Append(')');
        }
        sb.Append(')');
    }

    static void WriteLocalVarDecl(LocalVarDecl lvd, StringBuilder sb, int indent)
    {
        sb.Append("(local-var");
        if (lvd.IsNew) sb.Append(" new");
        if (lvd.IsStatic) sb.Append(" static");
        if (lvd.RefCount > 0) sb.Append(" ref");
        if (lvd.HasVar) sb.Append(" var");
        sb.Append($" {QuoteString(lvd.Name)})");
    }

    static void WriteInterpolatedString(InterpolatedString istr, StringBuilder sb, int indent)
    {
        sb.Append("(interp");
        foreach (var seg in istr.Segments)
        {
            sb.Append(' ');
            switch (seg)
            {
                case TextSegment ts: sb.Append(QuoteString(ts.Text)); break;
                case IdSegment ids: sb.Append($"(id {QuoteString(ids.Name)})"); break;
                case ExprSegment es: sb.Append("(expr "); Write(es.Expression, sb, indent + 2); sb.Append(')'); break;
            }
        }
        sb.Append(')');
    }

    static void WriteNewExpr(NewExpr newE, StringBuilder sb, int indent)
    {
        sb.Append("(new");
        sb.Append(' ');
        Write(newE.Type, sb, indent + 2);
        foreach (var a in newE.Args.Args)
        {
            sb.Append(' ');
            Write(a, sb, indent + 2);
        }
        sb.Append(')');
    }

    static void WriteLambdaExpr(LambdaExpr lam, StringBuilder sb, int indent)
    {
        sb.Append("(lambda");
        sb.Append(" (params");
        foreach (var p in lam.Params) { sb.Append(' '); Write(p, sb, indent + 4); }
        sb.Append(')');
        sb.Append('\n'); sb.Append(Pad(indent + 2));
        Write(lam.Body, sb, indent + 2);
        sb.Append(')');
    }

    static void WriteCoroutineExpr(CoroutineExpr cor, StringBuilder sb, int indent)
    {
        sb.Append("(coroutine");
        sb.Append(' ');
        Write(cor.Callee, sb, indent + 2);
        if (cor.Args.Length > 0)
        {
            foreach (var a in cor.Args) { sb.Append(' '); Write(a, sb, indent + 2); }
        }
        sb.Append(')');
    }

    static void WriteAsmBlock(StringBuilder sb, int indent, string tag, ImmutableArray<AsmInstr> instrs)
    {
        sb.Append($"({tag}");
        foreach (var i in instrs)
        {
            sb.Append('\n'); sb.Append(Pad(indent + 2));
            Write(i, sb, indent + 2);
        }
        sb.Append(')');
    }

    static void WriteAsmOpInstr(AsmOpInstr aoi, StringBuilder sb)
    {
        sb.Append($"(op {QuoteString(aoi.RawOpName)}");
        if (aoi.Arg0 != null) { sb.Append(' '); WriteAsmArg(aoi.Arg0, sb); }
        if (aoi.Arg1 != null) { sb.Append(' '); WriteAsmArg(aoi.Arg1, sb); }
        sb.Append(')');
    }

    static void WriteAsmArg(AsmArg arg, StringBuilder sb)
    {
        switch (arg)
        {
            case AsmArgInt ai: sb.Append(ai.Value.ToString()); break;
            case AsmArgReal ar: sb.Append(ar.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
            case AsmArgBool ab: sb.Append(ab.Value ? "true" : "false"); break;
            case AsmArgId aid: sb.Append(QuoteString(aid.Name)); break;
        }
    }

    static void WriteStaticCallExpr(StaticCallExpr sc, StringBuilder sb, int indent)
    {
        sb.Append("(static-call");
        sb.Append(' ');
        Write(sc.Type, sb, indent + 2);
        if (!string.IsNullOrEmpty(sc.Member)) { sb.Append(' '); sb.Append(QuoteString(sc.Member)); }
        if (sc.Args.HasExplicitParens)
        {
            foreach (var a in sc.Args.Args) { sb.Append(' '); Write(a, sb, indent + 2); }
        }
        sb.Append(')');
    }

    static void WriteCallExpr(CallExpr call, StringBuilder sb, int indent)
    {
        sb.Append("(call");
        sb.Append(' ');
        Write(call.Callee, sb, indent + 2);
        foreach (var a in call.Args.Args) { sb.Append(' '); Write(a, sb, indent + 2); }
        if (call.Args.PrependArgs.Length > 0)
        {
            sb.Append(" (prepend");
            foreach (var a in call.Args.PrependArgs) { sb.Append(' '); Write(a, sb, indent + 4); }
            sb.Append(')');
        }
        sb.Append(')');
    }

    static void WriteMemberCallExpr(MemberCallExpr mc, StringBuilder sb, int indent)
    {
        sb.Append("(member-call");
        sb.Append(' ');
        Write(mc.Subject, sb, indent + 2);
        sb.Append(' ');
        sb.Append(QuoteString(mc.Member));
        foreach (var a in mc.Args.Args) { sb.Append(' '); Write(a, sb, indent + 2); }
        sb.Append(')');
    }

    static void WriteIndirectCallExpr(IndirectCallExpr ic, StringBuilder sb, int indent)
    {
        sb.Append("(indirect-call");
        sb.Append(' ');
        Write(ic.Subject, sb, indent + 2);
        foreach (var a in ic.Args.Args) { sb.Append(' '); Write(a, sb, indent + 2); }
        sb.Append(')');
    }

    static void WriteAssignExpr(AssignExpr ae, StringBuilder sb, int indent)
    {
        var opName = ae.Op switch
        {
            AssignOp.Assign => "=",
            AssignOp.Add => "+=",
            AssignOp.Sub => "-=",
            AssignOp.Mul => "*=",
            AssignOp.Div => "/=",
            AssignOp.BitwiseAnd => "&=",
            AssignOp.BitwiseOr => "|=",
            AssignOp.Coalesce => "??=",
            AssignOp.DeltaLeft => "<<=",
            AssignOp.DeltaRight => ">>=",
            AssignOp.Cast => "~=",
            _ => "assign"
        };
        WriteList(sb, indent, opName, new Node[] { ae.Target, ae.Value });
    }

    static void WritePrxTypeExpr(PrxTypeExpr pt, StringBuilder sb)
    {
        if (pt.TypeArgs.IsEmpty)
        {
            sb.Append($"(type {QuoteString(pt.Name)})");
        }
        else
        {
            sb.Append($"(type {QuoteString(pt.Name)}<");
            for (int i = 0; i < pt.TypeArgs.Length; i++)
            {
                if (i > 0) sb.Append(',');
                switch (pt.TypeArgs[i])
                {
                    case TypeArgLiteral tal: sb.Append(tal.Value?.ToString() ?? "null"); break;
                    case TypeArgExpr tae: sb.Append("(expr "); Write(tae.Expression, sb, 0); sb.Append(')'); break;
                }
            }
            sb.Append(">)");
        }
    }

    static void WriteMExprAtom(MExprAtom mea, StringBuilder sb)
    {
        switch (mea.Value)
        {
            case null: sb.Append("null"); break;
            case string s: sb.Append(QuoteString(s)); break;
            case bool b: sb.Append(b ? "true" : "false"); break;
            case int i: sb.Append(i.ToString()); break;
            case double d: sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
            case System.Version v: sb.Append(v.ToString()); break;
            default: sb.Append(mea.Value.ToString() ?? "null"); break;
        }
    }

    static void WriteNsTransferSpec(NsTransferSpec nts, StringBuilder sb, int indent)
    {
        sb.Append("(ns-spec");
        sb.Append(' ');
        Write(nts.Source, sb, indent + 2);
        if (nts.SourceHasWildcard) sb.Append(" *");
        foreach (var d in nts.Directives) { sb.Append(' '); Write(d, sb, indent + 2); }
        sb.Append(')');
    }

    // ── Binary/Unary op names ─────────────────────────────────────────────────

    static string BinOpName(BinaryOp op) => op switch
    {
        BinaryOp.Add => "+",
        BinaryOp.Sub => "-",
        BinaryOp.Mul => "*",
        BinaryOp.Div => "/",
        BinaryOp.Mod => "mod",
        BinaryOp.Pow => "^",
        BinaryOp.Eq => "==",
        BinaryOp.Ne => "!=",
        BinaryOp.Lt => "<",
        BinaryOp.Le => "<=",
        BinaryOp.Gt => ">",
        BinaryOp.Ge => ">=",
        BinaryOp.LogicalAnd => "and",
        BinaryOp.LogicalOr => "or",
        BinaryOp.BitwiseAnd => "&&",
        BinaryOp.BitwiseOr => "||",
        BinaryOp.Xor => "xor",
        BinaryOp.DeltaLeft => "<|",
        BinaryOp.DeltaRight => "|>",
        BinaryOp.Then => "then",
        _ => op.ToString()
    };

    static string UnaryOpName(UnaryOp op) => op switch
    {
        UnaryOp.Negate => "neg",
        UnaryOp.UnaryPlus => "pos",
        UnaryOp.LogicalNot => "not",
        UnaryOp.PreIncrement => "pre++",
        UnaryOp.PreDecrement => "pre--",
        UnaryOp.PostIncrement => "post++",
        UnaryOp.PostDecrement => "post--",
        UnaryOp.PreDeltaLeft => "pre<|",
        UnaryOp.PreDeltaRight => "pre|>",
        UnaryOp.PostDeltaLeft => "post<|",
        UnaryOp.PostDeltaRight => "post|>",
        UnaryOp.Splice => "splice",
        _ => op.ToString()
    };

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void WriteList(StringBuilder sb, int indent, string tag, IEnumerable<Node?> childrenSeq)
    {
        var children = childrenSeq.ToArray();
        if (children.Length == 0)
        {
            sb.Append($"({tag})");
            return;
        }
        // Try inline first
        var inlineSb = new StringBuilder();
        inlineSb.Append($"({tag}");
        foreach (var c in children)
        {
            inlineSb.Append(' ');
            Write(c, inlineSb, 0);
        }
        inlineSb.Append(')');
        if (inlineSb.Length <= 80 && !inlineSb.ToString().Contains('\n'))
        {
            sb.Append(inlineSb);
            return;
        }
        // Multi-line
        sb.Append($"({tag}");
        foreach (var c in children)
        {
            sb.Append('\n');
            sb.Append(Pad(indent + 2));
            Write(c, sb, indent + 2);
        }
        sb.Append(')');
    }

    static string Pad(int indent) => indent > 0 ? new string(' ', indent) : "";

    public static string QuoteString(string s)
    {
        var sb = new StringBuilder("\"");
        foreach (char c in s)
        {
            if (c == '\\') sb.Append("\\\\");
            else if (c == '"') sb.Append("\\\"");
            else if (c == '\n') sb.Append("\\n");
            else if (c == '\r') sb.Append("\\r");
            else if (c == '\t') sb.Append("\\t");
            else sb.Append(c);
        }
        sb.Append('"');
        return sb.ToString();
    }

    // Private helper node type for embedding raw string fragments
    sealed record _StrNode(string Value) : Node(SourceSpan.Unknown);
}
