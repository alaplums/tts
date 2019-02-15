/* This file was generated by SableCC (http://www.sablecc.org/). */

package net.sourceforge.texlipse.texparser.node;

import net.sourceforge.texlipse.texparser.analysis.*;

@SuppressWarnings("nls")
public final class TCommentline extends Token
{
    public TCommentline(String text)
    {
        setText(text);
    }

    public TCommentline(String text, int line, int pos)
    {
        setText(text);
        setLine(line);
        setPos(pos);
    }

    @Override
    public Object clone()
    {
      return new TCommentline(getText(), getLine(), getPos());
    }

    public void apply(Switch sw)
    {
        ((Analysis) sw).caseTCommentline(this);
    }
}
